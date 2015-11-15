using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using com.esri.core.geometry;

namespace geometry_server_cs {
	public class GeomOpParts {
		private Operator.Type m_operatorType;
		private List<Geometry> m_leftGeometries = new List<Geometry>();

		private String[] m_leftGeometryStrings;
		private String m_operatorName;
		public String OperatorName { 
			get {
				return m_operatorName;
			}
			set {
				Operator.Type operatorType = (Operator.Type)Enum.Parse(typeof(Operator.Type), value);
				if (Enum.IsDefined(typeof(Operator.Type), operatorType) | operatorType.ToString().Contains(","))
					m_operatorType = operatorType;
				else 
					throw new JsonException(String.Format("{0} is not an operator defined in the Operator.Type enum"));
			}
		}
		public String[] LeftWKTGeometries { 
			get {
				return null;
			}
			set {
				foreach (String wkt in value) {
					m_leftGeometries.Add(GeometryEngine.GeometryFromWkt(wkt, 0, Geometry.Type.Unknown));
				}
			}
		}
		public GeomOpParts[] LeftGeometryOperations { get; set; }
		public String[] RightGeometries { get; set; }
		public GeomOpParts[] RightGeometryOperations { get; set; }
		private SpatialReference m_spatialReference = null;
		public String wkt_sr {
			set {
				m_spatialReference = SpatialReference.Create(value);
			}
		}

		public int wkid_sr {
			set {
				m_spatialReference = SpatialReference.Create(value);
			}
		}
		public double[] DoubleVariables { get; set; }
		public bool[] BoolVariables { get; set; }

		public String ExecuteOperator() {
			List<String> result = null;
			switch (m_operatorType) {
				case Operator.Type.Boundary:
					break;
				case Operator.Type.Buffer:
					result = GeometryEngine.Buffer(m_leftGeometries.ToArray(), m_spatialReference, DoubleVariables, BoolVariables == null ? false : BoolVariables[0]).Select(p => GeometryEngine.GeometryToWkt(p, 0)).ToList();
					break;
				case Operator.Type.Clip:
					break;
				case Operator.Type.Contains:
					break;
				case Operator.Type.ConvexHull:
					break;
				case Operator.Type.Crosses:
					break;
				case Operator.Type.Cut:
					break;
				case Operator.Type.DensifyByAngle:
					break;
				case Operator.Type.DensifyByLength:
					break;
				case Operator.Type.Difference:
					break;
				case Operator.Type.Disjoint:
					break;
				case Operator.Type.Distance:
					break;
				case Operator.Type.Equals:
					break;
				case Operator.Type.ExportToESRIShape:
					break;
				case Operator.Type.ExportToGeoJson:
					break;
				case Operator.Type.ExportToJson:
					break;
				case Operator.Type.ExportToWkb:
					break;
				case Operator.Type.ExportToWkt:
					break;
				case Operator.Type.Generalize:
					break;
				case Operator.Type.GeodesicBuffer:
					break;
				case Operator.Type.GeodeticArea:
					break;
				case Operator.Type.GeodeticDensifyByLength:
					break;
				case Operator.Type.GeodeticLength:
					break;
				case Operator.Type.ImportFromESRIShape:
					break;
				case Operator.Type.ImportFromGeoJson:
					break;
				case Operator.Type.ImportFromJson:
					break;
				case Operator.Type.ImportFromWkb:
					break;
				case Operator.Type.ImportFromWkt:
					break;
				case Operator.Type.ImportMapGeometryFromJson:
					break;
				case Operator.Type.Intersection:
					break;
				case Operator.Type.Intersects:
					break;
				case Operator.Type.LabelPoint:
					break;
				case Operator.Type.Offset:
					break;
				case Operator.Type.Overlaps:
					break;
				case Operator.Type.Project:
					break;
				case Operator.Type.Proximity2D:
					break;
				case Operator.Type.Relate:
					break;
				case Operator.Type.ShapePreservingDensify:
					break;
				case Operator.Type.Simplify:
					break;
				case Operator.Type.SimplifyOGC:
					break;
				case Operator.Type.SymmetricDifference:
					break;
				case Operator.Type.Touches:
					break;
				case Operator.Type.Union:
					break;
				case Operator.Type.Within:
					break;
				default:
					break;
			}
			return JsonConvert.SerializeObject(result); ;
		}

	}
}
