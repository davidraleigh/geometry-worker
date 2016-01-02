using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using com.esri.core.geometry;

namespace geometry_server {
	public class OperatorCursor {
		private Operator.Type m_operatorType;
		private GeometryCursor m_leftGeometrCurosr = null;
		private GeometryCursor m_rightGeometryCursor = null;
		private List<Geometry> m_leftGeometries = new List<Geometry>();
		private List<Geometry> m_rightGeometries = new List<Geometry>();
		private Envelope2D m_envelope2D = new Envelope2D();
		private List<Envelope> m_envelopes = new List<Envelope>();
		private OperatorOffset.JoinType m_operatorOffsetJoin;

		public String de_9im { get; set; }

		public String offset_join {
			set {
				OperatorOffset.JoinType operatorOffsetJoin = (OperatorOffset.JoinType)Enum.Parse(typeof(OperatorOffset.JoinType), value);
				if (Enum.IsDefined(typeof(OperatorOffset.JoinType), operatorOffsetJoin) | operatorOffsetJoin.ToString().Contains(","))
					m_operatorOffsetJoin = operatorOffsetJoin;
				else
					throw new JsonException(String.Format("{0} is not an operator defined in the Operator.Type enum", value));
			}
		}

		public String operator_name {
			set {
				Operator.Type operatorType = (Operator.Type)Enum.Parse(typeof(Operator.Type), value);
				if (Enum.IsDefined(typeof(Operator.Type), operatorType) | operatorType.ToString().Contains(","))
					m_operatorType = operatorType;
				else
					throw new JsonException(String.Format("{0} is not an operator defined in the Operator.Type enum", value));
			}
		}
		public String[] left_wkt_geometries {
			set {
				foreach (String wkt in value) {
					m_leftGeometries.Add(GeometryEngine.GeometryFromWkt(wkt, 0, Geometry.Type.Unknown));
				}
				m_leftGeometrCurosr = new SimpleGeometryCursor(m_leftGeometries);
			}
		}
		public String[] right_wkt_geometries {
			set {
				foreach (String wkt in value) {
					m_rightGeometries.Add(GeometryEngine.GeometryFromWkt(wkt, 0, Geometry.Type.Unknown));
				}
				m_rightGeometryCursor = new SimpleGeometryCursor(m_rightGeometries);
			}
		}

		public String envelope {
			set {
				GeometryEngine.GeometryFromWkt(value, 0, Geometry.Type.Unknown).QueryEnvelope2D(m_envelope2D);
			}
		}
		public OperatorCursor left_geometry_operations { get; set; }
		public OperatorCursor right_geometry_operations { get; set; }
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
		public double[] input_doubles { get; set; }
		public bool[] input_booleans { get; set; }
		public int[] input_integers { get; set; }

		public GeometryCursor GenerateCursor(ref double distance, ref bool spatialRelationship, ref List<Proximity2DResult> proximityResults) {
			if (m_leftGeometrCurosr == null && left_geometry_operations != null) {
				m_leftGeometrCurosr = left_geometry_operations.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
				if (m_leftGeometrCurosr == null)
					throw new JsonException(String.Format("left_geometry_operations must return a geometry, {0} does not return a geometry", Enum.GetName(typeof(Operator.Type), left_geometry_operations.m_operatorType)));
			}
			if (m_rightGeometryCursor == null && right_geometry_operations != null) {
				m_rightGeometryCursor = right_geometry_operations.GenerateCursor(ref distance, ref spatialRelationship, ref proximityResults);
				if (m_rightGeometryCursor == null)
					throw new JsonException(String.Format("right_geometry_operations must return a geometry, {0} does not return a geometry", Enum.GetName(typeof(Operator.Type), right_geometry_operations.m_operatorType)));
			}

			// TODO this might be useful
			//OperatorFactoryLocal.GetInstance().IsOperatorSupported(m_operatorType);

			GeometryCursor geometryCursor = null;
			distance = Double.NegativeInfinity;
			spatialRelationship = false;
			proximityResults = new List<Proximity2DResult>();

			// TODO check that default to false for input_booleans is correct
			switch (m_operatorType) {
			case Operator.Type.Boundary:
				geometryCursor = OperatorBoundary.Local().Execute(m_leftGeometrCurosr, null);
				break;
			case Operator.Type.Buffer: // "{operator_name:\"Buffer\", left_wkt_geometries:[\"POLYGON((1 1,5 1,5 5,1 5,1 1),(2 2, 3 2, 3 3, 2 3,2 2))\"], wkid_sr:4326, input_booleans:[false], input_doubles:[2.0]}"
				geometryCursor = OperatorBuffer.Local().Execute(m_leftGeometrCurosr, m_spatialReference, input_doubles, input_booleans == null ? false : input_booleans[0], null);
				break;
			case Operator.Type.Clip:
				geometryCursor = OperatorClip.Local().Execute(m_leftGeometrCurosr, m_envelope2D, m_spatialReference, null);
				break;
			case Operator.Type.Contains:
				spatialRelationship = OperatorContains.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.ConvexHull: // {operator_name:"ConvexHull", left_wkt_geometries:["POLYGON((1 1,5 1,5 5,1 5,1 1),(2 2, 3 2, 3 3, 2 3,2 2))"], input_booleans:[false]}
				geometryCursor = OperatorConvexHull.Local().Execute(m_leftGeometrCurosr, input_booleans == null ? false : input_booleans[0], null);
				break;
			case Operator.Type.Crosses:
				spatialRelationship = OperatorCrosses.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.Cut:
				geometryCursor = OperatorCut.Local().Execute(input_booleans == null ? false : input_booleans[0], m_leftGeometrCurosr.Next(), (Polyline)m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.DensifyByAngle:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.DensifyByLength:
				geometryCursor = OperatorDensifyByLength.Local().Execute(m_leftGeometrCurosr, input_doubles[0], null);
				break;
			case Operator.Type.Difference:
				geometryCursor = OperatorDifference.Local().Execute(m_leftGeometrCurosr, m_rightGeometryCursor, m_spatialReference, null);
				break;
			case Operator.Type.Disjoint:
				spatialRelationship = OperatorDisjoint.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.Distance:
				distance = OperatorDistance.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), null);
				break;
			case Operator.Type.Equals:
				spatialRelationship = OperatorEquals.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.ExportToESRIShape:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ExportToGeoJson:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ExportToJson:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ExportToWkb:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ExportToWkt:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.Generalize:
				geometryCursor = OperatorGeneralize.Local().Execute(m_leftGeometrCurosr, input_doubles[0], input_booleans == null ? false : input_booleans[0], null);
				break;
			case Operator.Type.GeodesicBuffer:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.GeodeticArea:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.GeodeticDensifyByLength:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.GeodeticLength:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportFromESRIShape:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportFromGeoJson:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportFromJson:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportFromWkb:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportFromWkt:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.ImportMapGeometryFromJson:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.Intersection:
				geometryCursor = OperatorIntersection.Local().Execute(m_leftGeometrCurosr, m_rightGeometryCursor, m_spatialReference, null, input_integers == null ? -1 : input_integers[0]);
				break;
			case Operator.Type.Intersects:
				spatialRelationship = OperatorIntersects.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.LabelPoint:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.Offset:
				geometryCursor = OperatorOffset.Local().Execute(m_leftGeometrCurosr, m_spatialReference, input_doubles[0], m_operatorOffsetJoin, input_doubles[1], input_doubles[2], null);
				break;
			case Operator.Type.Overlaps:
				spatialRelationship = OperatorOverlaps.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.Project:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.Proximity2D:
				if (input_booleans != null)
					proximityResults.Add(OperatorProximity2D.Local().GetNearestCoordinate(m_leftGeometrCurosr.Next(), (Point)m_rightGeometryCursor.Next(), input_booleans[0]));
				else if (input_doubles != null && input_integers != null)
					proximityResults.AddRange(OperatorProximity2D.Local().GetNearestVertices(m_leftGeometrCurosr.Next(), (Point)m_rightGeometryCursor.Next(), input_doubles[0], input_integers[0]));
				else
					proximityResults.Add(OperatorProximity2D.Local().GetNearestVertex(m_leftGeometrCurosr.Next(), (Point)m_rightGeometryCursor.Next()));
				break;
			case Operator.Type.Relate:
				spatialRelationship = OperatorRelate.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, de_9im, null);
				break;
			case Operator.Type.ShapePreservingDensify:
				throw new JsonException(String.Format("{0} Not a supported operation at this time", Enum.GetName(typeof(Operator.Type), m_operatorType)));
			case Operator.Type.Simplify:
				geometryCursor = OperatorSimplify.Local().Execute(m_leftGeometrCurosr, m_spatialReference, input_booleans == null ? false : input_booleans[0], null);
				break;
			case Operator.Type.SimplifyOGC:
				geometryCursor = OperatorSimplifyOGC.Local().Execute(m_leftGeometrCurosr, m_spatialReference, input_booleans == null ? false : input_booleans[0], null);
				break;
			case Operator.Type.SymmetricDifference:
				geometryCursor = OperatorSymmetricDifference.Local().Execute(m_leftGeometrCurosr, m_rightGeometryCursor, m_spatialReference, null);
				break;
			case Operator.Type.Touches:
				spatialRelationship = OperatorTouches.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			case Operator.Type.Union:
				if (m_rightGeometries.Count > 0) {
					m_leftGeometries.Add(m_rightGeometries[0]);
					m_leftGeometrCurosr = new SimpleGeometryCursor(m_leftGeometries);
				}
				geometryCursor = OperatorUnion.Local().Execute(m_leftGeometrCurosr, m_spatialReference, null);
				break;
			case Operator.Type.Within:
				spatialRelationship = OperatorWithin.Local().Execute(m_leftGeometrCurosr.Next(), m_rightGeometryCursor.Next(), m_spatialReference, null);
				break;
			default:
				throw new JsonException(String.Format("{0} Unknown operation", Enum.GetName(typeof(Operator.Type), m_operatorType)));
				break;
			}
			return geometryCursor;
		}

	}
}
