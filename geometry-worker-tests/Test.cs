using NUnit.Framework;
using System;
using com.esri.core.geometry;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using geometry_server;

namespace geometryworkertests
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void TestCase ()
		{
			List<String> enumStrings = GeometryEngineInfo.GeometryEngineMethods();
			Assert.IsNotNull(enumStrings);
			Assert.IsFalse(enumStrings.Count == 0);
			Assert.IsTrue(enumStrings.Contains("Buffer"));
			Assert.IsFalse(enumStrings.Contains("Project"));
		}

//		[Test ()]
//		public void MethodDetailsTest() {
//			JObject methodDetails = GeometryEngineInfo.OperatorInfo("Buffer");
//			Assert.IsNotNull(methodDetails);
//			Assert.IsTrue(methodDetails.HasValues);
//			Assert.AreEqual(methodDetails.First.Path, "opertorType");
//			Assert.AreEqual(methodDetails.First.Next.Path, "executeMethods");
//			Assert.AreEqual(methodDetails.First.First.Value<String>(), "Buffer");
//		}

		public List<String> UnionTest(String[] leftWKTs, int wkid) {
			String[] leftWKT = null;
			List<Geometry> m_leftGeometries = new List<Geometry>();
			foreach (String wkt in leftWKT) {
				m_leftGeometries.Add(GeometryEngine.GeometryFromWkt(wkt, 0, Geometry.Type.Unknown));
			}
			GeometryCursor geometryCursor = new SimpleGeometryCursor(m_leftGeometries);
			SpatialReference spatialReference = new SpatialReference(wkid);
			GeometryCursor unionGeometryCursor = OperatorUnion.Local().Execute(geometryCursor, m_spatialReference, null);

			Geometry geom = null;
			List<String> wktArray = new List<String>();

			while ((geom = unionGeometryCursor.Next()) != null) {
				wktArray.Add(GeometryEngine.GeometryToWkt(geom, 0));
			}

			return wktArray;
		}

		[Test ()]
		public void MethodDetailsTest2() {


			JObject methodDetails = GeometryEngineInfo.OperatorInfo("Proximity2D");
			String words = methodDetails.ToString();
			Assert.IsNotNull(methodDetails);
			Assert.IsTrue(methodDetails.HasValues);
			Assert.AreEqual(methodDetails.First.Path, "opertorType");
			Assert.AreEqual(methodDetails.First.Next.Path, "executeMethods");
			Assert.AreEqual(methodDetails.First.First.Value<String>(), "Buffer");
		}

		[Test ()]
		public void NotImplementedTest() {
			List<String> nonSupported = GeometryEngineInfo.NotImplemented();
			Assert.IsNotNull(nonSupported);
			Assert.IsFalse(nonSupported.Count == 0);
			Assert.IsTrue(nonSupported.Contains("ExportToJson"));
			Assert.IsTrue(nonSupported.Contains("Project"));
		}
	}
}

