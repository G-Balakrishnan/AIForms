using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureAIForms.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InvoiceReader()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> UploadInvoice(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            var filePaths = new List<string>();



             string endpoint = "https://menporul.cognitiveservices.azure.com/";
             string apiKey = "78ad55b53bf5442eabbb6e987e87047f";

            var credential = new AzureKeyCredential(apiKey);
            var client = new FormRecognizerClient(new Uri(endpoint), credential);
            // full path to file in temp location
            var filePath = AppDomain.CurrentDomain.BaseDirectory + HttpContext.Request.Form.Files[0].FileName ;
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
           //we are using Temp file name just for the example. Add your own file path.
            filePaths.Add(filePath);
            var stream = new FileStream(filePath, FileMode.Create);
            
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }

            //string invoicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoice_1.pdf");
            stream.Close();

            var options = new RecognizeInvoicesOptions() { Locale = "en-US" };
            // using var stream1 = new FileStream(@"C:\Users\91848\Source\Repos\AI_Form_Reader\AI_Form_Reader\"+ "Invoice_1.pdf", FileMode.Open);

            using var stream1 = new FileStream(filePath, FileMode.Open);
            RecognizeInvoicesOperation operation = client.StartRecognizeInvoicesAsync(stream1, options).Result;
            Response<RecognizedFormCollection> operationResponse = operation.WaitForCompletionAsync().Result;
            RecognizedFormCollection invoices = operationResponse.Value;

          

            RecognizedForm invoice = invoices.Single();
            ExpandoObject obj = new ExpandoObject();
            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            foreach (var item in invoice.Fields)
            {
                obj.TryAdd<string, object>(item.Key,item.Value?.ValueData?.Text);
                data.Add(new KeyValuePair<string, string>(item.Key, item.Value?.ValueData?.Text));
            }

            //if (invoice.Fields.TryGetValue("VendorName", out FormField vendorNameField))
            //{
            //    if (vendorNameField.Value.ValueType == FieldValueType.String)
            //    {
            //        string vendorName = vendorNameField.Value.AsString();
            //        Console.WriteLine($"Vendor Name: '{vendorName}', with confidence {vendorNameField.Confidence}");
            //    }
            //}

            //if (invoice.Fields.TryGetValue("CustomerName", out FormField customerNameField))
            //{
            //    if (customerNameField.Value.ValueType == FieldValueType.String)
            //    {
            //        string customerName = customerNameField.Value.AsString();
            //        Console.WriteLine($"Customer Name: '{customerName}', with confidence {customerNameField.Confidence}");
            //    }
            //}
            //foreach (var formFile in files)
            //{
            //    if (formFile.Length > 0)
            //    {
            //        // full path to file in temp location
            //        var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
            //        filePaths.Add(filePath);
            //        using (var stream = new FileStream(filePath, FileMode.Create))
            //        {
            //            await formFile.CopyToAsync(stream);
            //        }
            //    }
            //}
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
            return Ok(data);
        }
    }
}
