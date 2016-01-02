using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.esri.core.geometry;
using Newtonsoft.Json.Linq;

namespace geometry_server {
    public class GeometryEngineInfo {

        public static List<String> GeometryEngineMethods() {
            List<String> geometryOperators = new List<String>();
            List<String> notImplemented = NotImplemented();
            foreach (Operator.Type operatorType in Enum.GetValues(typeof(Operator.Type))) {
                String operatorName = Enum.GetName(typeof(Operator.Type), operatorType);
                if (notImplemented.Contains(operatorName))
                    continue;

                geometryOperators.Add(operatorName);
            }

            return geometryOperators;
        }


        public static List<String> NotImplemented() {
            List<String> nonSupported = new List<String>();
            foreach (Operator.Type operatorType in Enum.GetValues(typeof(Operator.Type))) {
                if (!OperatorFactoryLocal.GetInstance().IsOperatorSupported(operatorType)) {
                    nonSupported.Add(Enum.GetName(typeof(Operator.Type), operatorType));
                } 
            }
            return nonSupported;
        }


        public static JObject OperatorInfo(String operatorTypeEnum) {
            Operator.Type operatorType = (Operator.Type)Enum.Parse(typeof(Operator.Type), operatorTypeEnum);
            System.Reflection.MethodInfo[] methodInfo = null;
            if (Enum.IsDefined(typeof(Operator.Type), operatorType) | operatorType.ToString().Contains(",")) {
				methodInfo = typeof(OperatorProximity2D).GetMethods();
            } else {
                throw new Exception(String.Format("{0} is not an operator defined in the Operator.Type enum", operatorTypeEnum));
            }

            Newtonsoft.Json.Linq.JObject operatorInfoObj = new Newtonsoft.Json.Linq.JObject();
            Newtonsoft.Json.Linq.JArray executeArray = new Newtonsoft.Json.Linq.JArray();
            foreach (System.Reflection.MethodInfo mi in methodInfo) {
				if (mi.Name != "Execute" && 
					mi.Name != "GetNearestCoordinate" && 
					mi.Name != "GetNearestVertex" &&
					mi.Name != "GetNearestVertices")
                    continue;

                Newtonsoft.Json.Linq.JObject executeMethod = new JObject();
                executeMethod.Add("returnType", mi.ReturnType.Name);

                Newtonsoft.Json.Linq.JArray parameterArray = new JArray();
                System.Reflection.ParameterInfo[] paramaterInfo = mi.GetParameters();
                foreach (System.Reflection.ParameterInfo pi in paramaterInfo) {
                    Newtonsoft.Json.Linq.JObject parameterObj = new Newtonsoft.Json.Linq.JObject();
                    parameterObj.Add(new JProperty("parameterPosition", pi.Position));
                    parameterObj.Add(new JProperty("parameterType",  pi.ParameterType.Name));
                    parameterObj.Add(new JProperty("parameterName", pi.Name));
                    parameterArray.Add(parameterObj);
                }
                executeMethod.Add(new JProperty("parameters", parameterArray));
                executeArray.Add(executeMethod);
            }
            operatorInfoObj.Add(new JProperty("opertorType", operatorTypeEnum));
            operatorInfoObj.Add(new JProperty("executeMethods", executeArray));

            return operatorInfoObj;
        }

    }
}