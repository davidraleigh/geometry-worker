using System;
using System.Threading;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

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
			GeometryOperator geomOpParts = JsonConvert.DeserializeObject<GeometryOperator>(e.Data);
			String result = geomOpParts.ExecuteOperator();
			this.Context.WebSocket.Send(String.Format("{0}: {1}", m_name, result));
		}
	}
}
