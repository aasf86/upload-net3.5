using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ApoloFileTransfer
{
    //https://www.paraesthesia.com/archive/2009/12/16/posting-multipartform-data-using-.net-webrequest.aspx/
    public static class DictionaryExtensions
    {
        public const string FormDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";

        public static void WriteMultipartFormData(
          this Dictionary<string, object> dictionary,
          Stream stream,
          string mimeBoundary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return;
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (mimeBoundary == null)
            {
                throw new ArgumentNullException("mimeBoundary");
            }
            if (mimeBoundary.Length == 0)
            {
                throw new ArgumentException("MIME boundary may not be empty.", "mimeBoundary");
            }
            foreach (string key in dictionary.Keys)
            {
                string item = String.Format(FormDataTemplate, mimeBoundary, key, dictionary[key]);
                byte[] itemBytes = System.Text.Encoding.UTF8.GetBytes(item);
                stream.Write(itemBytes, 0, itemBytes.Length);
            }
        }
    }

    public static class FileInfoExtensions
    {
        public const string HeaderTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";

        public static void WriteMultipartFormData(
          this FileInfo file,
          Stream stream,
          string mimeBoundary,
          string mimeType,
          string formKey)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("Unable to find file to write to stream.", file.FullName);
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (mimeBoundary == null)
            {
                throw new ArgumentNullException("mimeBoundary");
            }
            if (mimeBoundary.Length == 0)
            {
                throw new ArgumentException("MIME boundary may not be empty.", "mimeBoundary");
            }
            if (mimeType == null)
            {
                throw new ArgumentNullException("mimeType");
            }
            if (mimeType.Length == 0)
            {
                throw new ArgumentException("MIME type may not be empty.", "mimeType");
            }
            if (formKey == null)
            {
                throw new ArgumentNullException("formKey");
            }
            if (formKey.Length == 0)
            {
                throw new ArgumentException("Form key may not be empty.", "formKey");
            }
            string header = String.Format(HeaderTemplate, mimeBoundary, formKey, file.Name, mimeType);
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            stream.Write(headerbytes, 0, headerbytes.Length);
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }
            byte[] newlineBytes = Encoding.UTF8.GetBytes("\r\n");
            stream.Write(newlineBytes, 0, newlineBytes.Length);
        }
    }

    public static class Run
    {
        private static string CreateFormDataBoundary()
        {
            return "---------------------------" + DateTime.Now.Ticks.ToString("x");
        }

        public static string ExecutePostRequest(
          Uri url,
          Dictionary<string, object> postData,
          FileInfo fileToUpload,
          string fileMimeType,
          string fileFormKey
        )
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.AbsoluteUri);
            request.Headers.Add("HTTP_ACCEPT", "application/json");
            request.Method = "POST";
            request.KeepAlive = true;
            string boundary = CreateFormDataBoundary();
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            Stream requestStream = request.GetRequestStream();
            postData.WriteMultipartFormData(requestStream, boundary);
            if (fileToUpload != null)
            {
                fileToUpload.WriteMultipartFormData(requestStream, boundary, fileMimeType, fileFormKey);
            }
            byte[] endBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            requestStream.Write(endBytes, 0, endBytes.Length);
            requestStream.Close();
            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            };
        }
    }
}
