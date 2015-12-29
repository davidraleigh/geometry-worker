using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using geometry_server;
using com.esri.core.geometry;
using System.Collections.Generic;


class RPCServer {
    public static void Main() {
        var factory = new ConnectionFactory();
        factory.HostName = "geometry-mq.cloudapp.net";
        //@"host=geometry-mq.cloudapp.net;username=geometry-cs;password=testpassword";
        factory.UserName = "geometry-cs";
        factory.Password = "testpassword";

		using (var connection = factory.CreateConnection()) {
        	using (var channel = connection.CreateModel()) {
				// remote procedure call queue
	            channel.QueueDeclare(queue: "rpc_queue",
	                                 durable: false,
	                                 exclusive: false,
	                                 autoDelete: false,
	                                 arguments: null);
				// fetch only one item off the queue at a time
	            channel.BasicQos(0, 1, false);

	            var consumer = new QueueingBasicConsumer(channel);
	            channel.BasicConsume(queue: "rpc_queue",
	                                 noAck: false,
	                                 consumer: consumer);
	            Console.WriteLine(" [x] Awaiting RPC requests");

				while (true) {
					var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

					var body = ea.Body;
					var props = ea.BasicProperties;
					var replyProps = channel.CreateBasicProperties();
					replyProps.CorrelationId = props.CorrelationId;

					try {
						var message = Encoding.UTF8.GetString(body);
						processGeometry(message, channel, replyProps, props);
					} catch (Exception e) {
						Console.WriteLine(" [.] " + e.Message);
					} finally {
						channel.BasicAck(deliveryTag: ea.DeliveryTag,
							multiple: false);
					}
				}
            }
        }
    }

	private static void processGeometry(String request, IModel channel, IBasicProperties replyProps, IBasicProperties props) {
		//TODO replace with id from rabbitMQ task?
		Newtonsoft.Json.Linq.JObject jobject = new Newtonsoft.Json.Linq.JObject();//new JProperty("RequestId", "test"));
		byte[] responseBytes = null;
		try {
			OperatorCursor geomOpParts = JsonConvert.DeserializeObject<OperatorCursor>(request);
			double distance = Double.NegativeInfinity;
			bool spatialRelationship = false;
			List<Proximity2DResult> proximityResults = null;

			GeometryCursor geomCursor = geomOpParts.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
			if (geomCursor != null) {
				Geometry geom = null;
				while ((geom = geomCursor.Next()) != null) {
					jobject = new JObject(
						new JProperty("geometry_results", GeometryEngine.GeometryToWkt(geom, 0))
					);

					responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobject, Formatting.Indented));
					channel.BasicPublish(exchange: "",
						routingKey: props.ReplyTo,
						basicProperties: replyProps,
						body: responseBytes);
				}
				return;
			} else if (proximityResults.Count > 0) {
				foreach (Proximity2DResult prox in proximityResults) {
					jobject = new JObject( new JProperty("proximity_results", JsonConvert.SerializeObject(prox)));
					responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobject, Formatting.Indented));
					channel.BasicPublish(exchange: "",
						routingKey: props.ReplyTo,
						basicProperties: replyProps,
						body: responseBytes);
				}
				return;
			} else if (!Double.IsNegativeInfinity(distance)) {
				jobject.Add("distance_results", distance);
			} else {
				jobject.Add("spatial_results", spatialRelationship);
			}
		} catch (Exception e) {
			jobject.Add("Error", e.Message);
		}
		responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobject, Formatting.Indented));
		channel.BasicPublish(exchange: "",
			routingKey: props.ReplyTo,
			basicProperties: replyProps,
			body: responseBytes);
	}
}