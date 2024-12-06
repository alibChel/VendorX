using Newtonsoft.Json;
using P42.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using ZXing.QrCode.Internal;

namespace VendorX.Services
{
    public static class GosyncServise
    {

        private static string Host = @"https://gosync.gosu.kz/"; //test
     


        private static string ServiceName = "Vendor";
        /// <summary>
        /// privet cherniy :)
        /// </summary>
        private static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", ServiceName);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 1&Tff$S7*0A4&IV7ahXkI3v@IavvGtZXa*qXFnwhSd1vf9gkCF");
         
           /// client.DefaultRequestHeaders.Add("app-token", $"{}");

            return client;
        }
        public static async Task<string> Register(string handle, string password, AuthType auth)
        {
            var client = GetClient();
            
            var json_content = new { handle = handle.Replace(" ",""), password = password, authType = auth };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var temp = await response.Content.ReadAsStringAsync();
                    return temp.Replace("\"", "");
                }
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }


        }
        public static async Task<string> Login(string handle, string password, AuthType auth)
         {
            handle = handle.RemoveWhitespace();
            var client = GetClient();
            var json_content = new { handle = handle,password=password, authType = auth };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var temp = await response.Content.ReadAsStringAsync();
                    return temp.Replace("\"", "");
                }
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());
              
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

           
        }  
        
        
    
        public static async Task<string> Login(string handle, AuthType auth)
        {
            var client = GetClient();
            var json_content = new { handle = handle, authType = auth};
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var temp = await response.Content.ReadAsStringAsync();
                    return temp.Replace("\"", "");
                }
                  
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

        }
        public static async Task<bool> SendEmailCode(string email, string login)
        {
            
            var client = GetClient();
            var json_content = new
            {
                templateName = "VendorSendEmailCode",
                serviceName = ServiceName,
                pairs = new Dictionary<string, string>()
                {
                    { "HANDLE", login },
                    { "CODE", null },
                    { "APP_NAME", ServiceName },

                },
                senderName = email,
                ConfirmUserExist = true,
                subject = "Ваша команда Vendor"
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/sendEmail", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                    throw new Exception(await response.Content.ReadAsStringAsync());


            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

        }

        public static async Task<bool> SendEmailCode(string email,string login,bool confirm) {
  
            var client = GetClient();
            var json_content = new {
                templateName = "VendorSendEmailCode",
                serviceName = ServiceName,
                pairs = new Dictionary<string, string>()
                {
                    { "HANDLE", login },
                    { "CODE", null },
                    { "APP_NAME", ServiceName },

                },
                senderName = email,
                ConfirmUserExist = confirm,
               subject = "Ваша команда Vendor"
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/sendEmail", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                   return false;
                  
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

        }

        public static async Task<bool> SendSmsCode(string number, string message) {
            //телефон пример 7 777 321 5912
            var client = GetClient();
            var json_content = new
            {
                phone = number,
                message = message,
                serviceName = ServiceName
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/sendSms", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public static async Task<bool> SendSmsCode(string number, string message,bool confirm)
        {
            //телефон пример 7 777 321 5912
            var client = GetClient();
            var json_content = new
            {
                phone = number,
                message = message,
                serviceName = ServiceName,
                 ConfirmUserExist = confirm
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/sendSms", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public static async Task<bool> ConfirmEmailCode(string code, string email)
        {

            var client = GetClient();
            var json_content = new
            {
            id= email,
            smsCode=code
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/verify", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }   
                
                else
                    return false;

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

        }
        public static async Task<bool> ConfirmSmsCode(string code,string number)
        {

            var client = GetClient();
            var json_content = new
            {
                id = number,
                smsCode = code
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/notification/verify", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                    return false;

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

        }

        public static async Task<bool> ResetPswd(string handle, string newpswd,AuthType authType)
        {
            //телефон пример 7 777 321 5912
            var client = GetClient();
            var json_content = new
            {
                handle = handle,
                newPassword = newpswd,
                authType = authType
            };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PatchAsync($"{Host}api/user/resetPassword", content);

                if (response.IsSuccessStatusCode)
                    return bool.Parse(await response.Content.ReadAsStringAsync());
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public static async Task<bool> DeleteAccount(string handle)
        {
            handle = handle.RemoveWhitespace();
            var client = GetClient();
            try
            {
                var response = await client.DeleteAsync($"{Host}api/user/deleteAccount?handle={handle}");

                if (response.IsSuccessStatusCode)
                {
           
                    return true;
                }
                else
                    return false;

            }
            catch 
            {
                return false;
            }


        }


        public static async Task<bool> BindHandle(string handle,string bindHandle, string bindPassword, AuthType auth)
        {
            handle = handle.RemoveWhitespace();
            var client = GetClient();
            var json_content = new { handle = handle, bindedHandle=bindHandle, bindedPassword = bindPassword, authType = auth };
            var json = JsonConvert.SerializeObject(json_content);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{Host}api/user/bindHandle", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                    return false;

            }
            catch 
            {
                return false;
            }


        }
    }
}
