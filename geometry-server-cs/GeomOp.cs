using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using com.esri.core.geometry;

namespace geometry_server_cs {
	public class GeomOp : WebSocketBehavior {
		private int m_id;
		private string m_name;
		private static int m_number = 0;
		private string m_prefix;

		public GeomOp()
			: this(null) {

		}

		public GeomOp(string prefix) {
			m_prefix = !prefix.IsNullOrEmpty() ? prefix : "anon#";
		}

		private string getName() {
			String name = Context.QueryString["name"];
			return !name.IsNullOrEmpty()
				? name : (m_prefix + getNumber());
		}

		private static int getNumber() {
			return Interlocked.Increment(ref m_number);
		}

		protected override void OnOpen() {
			m_name = getName();
		}

		protected override void OnMessage(MessageEventArgs e) {
			OperatorCursor geomOpParts = JsonConvert.DeserializeObject<OperatorCursor>(e.Data);
			double distance = Double.NegativeInfinity;
			bool spatialRelationship = false;
			List<Proximity2DResult> proximityResults = null;

			GeometryCursor geomCursor = geomOpParts.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
			if (geomCursor != null) {
				Geometry geom = null;
				while ((geom = geomCursor.Next()) != null) {
					String wkt = JsonConvert.SerializeObject(GeometryEngine.GeometryToWkt(geom, 0));
					this.Context.WebSocket.Send(String.Format("user:{0}, result:{1}", m_name, wkt));
				}
			} else if (proximityResults != null) {
				foreach (Proximity2DResult prox in proximityResults) {
					this.Context.WebSocket.Send(String.Format("{user:{0}, result:{1}}", m_name, JsonConvert.SerializeObject(prox)));
				}
			} else if (!Double.IsNegativeInfinity(distance)) {
				this.Context.WebSocket.Send(String.Format("{user:{0}, result:{1}}", m_name, distance));
			} else {
				this.Context.WebSocket.Send(String.Format("{user:{0}, result:{1}}", m_name, spatialRelationship));
			}
		}
	}
}
