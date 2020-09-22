using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ApoloFileTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = "ApoloEletronico.pdf";
            var fileUri = "C:\\Users\\aasf_\\Downloads\\" + fileName;            
            
            Dictionary<string, object> postParameters = new Dictionary<string, object>();            
            postParameters.Add("descricao[]", "arquivo-betoides");
            postParameters.Add("ordem[]", "01");
            postParameters.Add("marcador[]", "apolo-desktop");            
            
            string postURL = "http://localhost:15788/Upload/FileTransferHandler.ashx?comarca=28";

            var resp = Run.ExecutePostRequest(new Uri(postURL), postParameters, new FileInfo(fileUri), "application/pdf", Guid.NewGuid().ToString());

            Console.WriteLine(resp);
        }
    }
}
