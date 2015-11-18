using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.esri.core.geometry;

namespace geometry_server {
    public class GeometryEngineInfo {

        public static List<String> GeometryEngineMethods() {
            List<String> geometryOperators = new List<String>();
            foreach (Operator.Type foo in Enum.GetValues(typeof(Operator.Type))) {
                geometryOperators.Add(Enum.GetName(typeof(Operator.Type), foo));
            }
            //System.Reflection.MethodInfo[] methodInfo = typeof(OperatorBuffer).GetMethods();
            //System.Reflection.MethodInfo[] mb = typeof(OperatorBufferLocal).GetMethods();
            return geometryOperators;
        }

        public static List<String> OperatorInfo(String operatorTypeEnum) {
            Operator.Type operatorType = (Operator.Type)Enum.Parse(typeof(Operator.Type), operatorTypeEnum);
            System.Reflection.MethodInfo[] methodInfo = null;
            if (Enum.IsDefined(typeof(Operator.Type), operatorType) | operatorType.ToString().Contains(",")) {
                methodInfo = typeof(OperatorBuffer).GetMethods();
            } else {
                throw new Exception(String.Format("{0} is not an operator defined in the Operator.Type enum", operatorTypeEnum));
            }

            List<String> operatorInfo = new List<String>();
            methodInfo.Where(mi => mi.Name == "Execute")
                .ToList()
                .ForEach(mi => operatorInfo.Add(mi.Name));
            return operatorInfo;
        }

    }
}