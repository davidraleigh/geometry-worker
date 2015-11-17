using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using com.esri.core.geometry;

namespace geometry_server {
    public class GeometryHub : Hub {
        public void Send(string name, string message) {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);

            OperatorCursor geomOpParts = JsonConvert.DeserializeObject<OperatorCursor>(message);
            double distance = Double.NegativeInfinity;
            bool spatialRelationship = false;
            List<Proximity2DResult> proximityResults = null;

            GeometryCursor geomCursor = geomOpParts.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
            if (geomCursor != null) {
                Geometry geom = null;
                while ((geom = geomCursor.Next()) != null) {
                    String wkt = JsonConvert.SerializeObject(GeometryEngine.GeometryToWkt(geom, 0));
                    Clients.All.broadcastMessage(String.Format("user:{0}, result:{1}", name, wkt));
                }
            } else if (proximityResults != null) {
                foreach (Proximity2DResult prox in proximityResults) {
                    Clients.All.broadcastMessage(String.Format("{user:{0}, result:{1}}", name, JsonConvert.SerializeObject(prox)));
                }
            } else if (!Double.IsNegativeInfinity(distance)) {
                Clients.All.broadcastMessage(String.Format("{user:{0}, result:{1}}", name, distance));
            } else {
                Clients.All.broadcastMessage(String.Format("{user:{0}, result:{1}}", name, spatialRelationship));
            }
        }
    }
}