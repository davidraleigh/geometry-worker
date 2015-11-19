using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geometry_worker {
    public class RabbitClusterAzure {
        public const string ConnectionString =
            @"host=geometry-mq.cloudapp.net;username=geometry-cs;password=testpassword";
    }

    class Request {
        public string Text { get; set; } 
    }
}
