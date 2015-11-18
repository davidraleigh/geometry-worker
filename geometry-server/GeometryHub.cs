using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.esri.core.geometry;

namespace geometry_server {
    public class GeometryHub : Hub {

        public void GetValidOperators() {
            Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(GeometryEngineInfo.GeometryEngineMethods()));
        }

        public void GetInValidOperators() {
            Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(GeometryEngineInfo.NotImplemented()));
        }

        public void GetOperatorDetails(String operatorTypeName) {
            Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(GeometryEngineInfo.OperatorInfo(operatorTypeName)));
        }

        public void GetAllOperatorDetails() {
            JArray obj = new JArray();
            List<String> validOperators = GeometryEngineInfo.GeometryEngineMethods();
            foreach (String enumTypeName in validOperators) {
                obj.Add(GeometryEngineInfo.OperatorInfo(enumTypeName));
            }
            Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(obj));
        }

        public void Request(string requestId, string requestContent) {
            // Call the broadcastMessage method to update clients.
            Clients.Caller.broadcastMessage(requestId, requestContent);
            Newtonsoft.Json.Linq.JObject jobject = new Newtonsoft.Json.Linq.JObject(new JProperty("RequestId", requestId));
            try {
                OperatorCursor geomOpParts = JsonConvert.DeserializeObject<OperatorCursor>(requestContent);
                double distance = Double.NegativeInfinity;
                bool spatialRelationship = false;
                List<Proximity2DResult> proximityResults = null;

                GeometryCursor geomCursor = geomOpParts.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
                if (geomCursor != null) {
                    Geometry geom = null;
                    while ((geom = geomCursor.Next()) != null) {
                        jobject = new JObject(
                                new JProperty("RequestId", requestId),
                                new JProperty("Results", GeometryEngine.GeometryToWkt(geom, 0))
                            );
                        Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(jobject));
                    }
                    return;
                } else if (proximityResults != null) {
                    foreach (Proximity2DResult prox in proximityResults) {
                        jobject = new JObject( new JProperty("RequestId", requestId),
                            new JProperty("Results", JsonConvert.SerializeObject(prox)));
                        Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(jobject));
                    }
                    return;
                } else if (!Double.IsNegativeInfinity(distance)) {
                    jobject.Add("Results", distance);
                } else {
                    jobject.Add("Results", spatialRelationship);
                }
            } catch (Exception e) {
                jobject.Add("Error", e.Message);
            }
            Clients.Caller.broadcastMessage(JsonConvert.SerializeObject(jobject));
        }
    }
}