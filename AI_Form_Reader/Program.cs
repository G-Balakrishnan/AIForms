using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI_Form_Reader
{
    class Program
    {
       static string endpoint = "https://menporul.cognitiveservices.azure.com/";
        static string apiKey = "78ad55b53bf5442eabbb6e987e87047f";
    
        static  void Main(string[] args)
        {
             var credential = new AzureKeyCredential(apiKey);
             var client = new FormRecognizerClient(new Uri(endpoint), credential);


            string invoicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoice_1.pdf");

            using var stream = new FileStream(invoicePath, FileMode.Open);
            var options = new RecognizeInvoicesOptions() { Locale = "en-US" };

            RecognizeInvoicesOperation operation =  client.StartRecognizeInvoicesAsync(stream, options).Result;
            Response<RecognizedFormCollection> operationResponse =  operation.WaitForCompletionAsync().Result;
            RecognizedFormCollection invoices = operationResponse.Value;

            // To see the list of all the supported fields returned by service and its corresponding types, consult:
            // https://aka.ms/formrecognizer/invoicefields

            RecognizedForm invoice = invoices.Single();

            if (invoice.Fields.TryGetValue("VendorName", out FormField vendorNameField))
            {
                if (vendorNameField.Value.ValueType == FieldValueType.String)
                {
                    string vendorName = vendorNameField.Value.AsString();
                    Console.WriteLine($"Vendor Name: '{vendorName}', with confidence {vendorNameField.Confidence}");
                }
            }

            if (invoice.Fields.TryGetValue("CustomerName", out FormField customerNameField))
            {
                if (customerNameField.Value.ValueType == FieldValueType.String)
                {
                    string customerName = customerNameField.Value.AsString();
                    Console.WriteLine($"Customer Name: '{customerName}', with confidence {customerNameField.Confidence}");
                }
            }

            if (invoice.Fields.TryGetValue("Items", out FormField itemsField))
            {
                if (itemsField.Value.ValueType == FieldValueType.List)
                {
                    foreach (FormField itemField in itemsField.Value.AsList())
                    {
                        Console.WriteLine("Item:");

                        if (itemField.Value.ValueType == FieldValueType.Dictionary)
                        {
                            IReadOnlyDictionary<string, FormField> itemFields = itemField.Value.AsDictionary();

                            if (itemFields.TryGetValue("Description", out FormField itemDescriptionField))
                            {
                                if (itemDescriptionField.Value.ValueType == FieldValueType.String)
                                {
                                    string itemDescription = itemDescriptionField.Value.AsString();

                                    Console.WriteLine($"  Description: '{itemDescription}', with confidence {itemDescriptionField.Confidence}");
                                }
                            }

                            if (itemFields.TryGetValue("Quantity", out FormField itemQuantityField))
                            {
                                if (itemQuantityField.Value.ValueType == FieldValueType.Float)
                                {
                                    float quantityAmount = itemQuantityField.Value.AsFloat();

                                    Console.WriteLine($"  Quantity: '{quantityAmount}', with confidence {itemQuantityField.Confidence}");
                                }
                            }

                            if (itemFields.TryGetValue("UnitPrice", out FormField itemUnitPriceField))
                            {
                                if (itemUnitPriceField.Value.ValueType == FieldValueType.Float)
                                {
                                    float itemUnitPrice = itemUnitPriceField.Value.AsFloat();

                                    Console.WriteLine($"  UnitPrice: '{itemUnitPrice}', with confidence {itemUnitPriceField.Confidence}");
                                }
                            }

                            if (itemFields.TryGetValue("Tax", out FormField itemTaxPriceField))
                            {
                                if (itemTaxPriceField.Value.ValueType == FieldValueType.Float)
                                {
                                    try
                                    {
                                        float itemTax = itemTaxPriceField.Value.AsFloat();
                                        Console.WriteLine($"  Tax: '{itemTax}', with confidence {itemTaxPriceField.Confidence}");
                                    }
                                    catch
                                    {
                                        string itemTaxText = itemTaxPriceField.ValueData.Text;
                                        Console.WriteLine($"  Tax: '{itemTaxText}', with confidence {itemTaxPriceField.Confidence}");
                                    }
                                }
                            }

                            if (itemFields.TryGetValue("Amount", out FormField itemAmountField))
                            {
                                if (itemAmountField.Value.ValueType == FieldValueType.Float)
                                {
                                    float itemAmount = itemAmountField.Value.AsFloat();

                                    Console.WriteLine($"  Amount: '{itemAmount}', with confidence {itemAmountField.Confidence}");
                                }
                            }
                        }
                    }
                }
            }

            if (invoice.Fields.TryGetValue("SubTotal", out FormField subTotalField))
            {
                if (subTotalField.Value.ValueType == FieldValueType.Float)
                {
                    float subTotal = subTotalField.Value.AsFloat();
                    Console.WriteLine($"Sub Total: '{subTotal}', with confidence {subTotalField.Confidence}");
                }
            }

            if (invoice.Fields.TryGetValue("TotalTax", out FormField totalTaxField))
            {
                if (totalTaxField.Value.ValueType == FieldValueType.Float)
                {
                    float totalTax = totalTaxField.Value.AsFloat();
                    Console.WriteLine($"Total Tax: '{totalTax}', with confidence {totalTaxField.Confidence}");
                }
            }

            if (invoice.Fields.TryGetValue("InvoiceTotal", out FormField invoiceTotalField))
            {
                if (invoiceTotalField.Value.ValueType == FieldValueType.Float)
                {
                    float invoiceTotal = invoiceTotalField.Value.AsFloat();
                    Console.WriteLine($"Invoice Total: '{invoiceTotal}', with confidence {invoiceTotalField.Confidence}");
                }
            }
            Console.WriteLine("Hello World!");
        }
    }
}
