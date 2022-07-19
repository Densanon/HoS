// BEFORE RUNNING:
// ---------------
// 1. If not already done, enable the Google Sheets API
//    and check the quota for your project at
//    https://console.developers.google.com/apis/api/sheets
// 2. Install the C# client library by adding a dependency on the relevant NuGet
//    package. Libraries published by Google are owned by google-apis-packages:
//    https://www.nuget.org/profiles/google-apis-packages

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Data = Google.Apis.Sheets.v4.Data;

public class SheetReader
{
    static private string jsonPath = "/Resources/Creds-key.json";

    public static List<string[]> GetSheetData()
    {
        string fullJsonPath = Application.dataPath + jsonPath;
        Stream jsonCreds = File.Open(fullJsonPath, FileMode.Open);
        ServiceAccountCredential credential = ServiceAccountCredential.FromServiceAccountData(jsonCreds);


        SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });

        // The ID of the spreadsheet to retrieve data from.
        string spreadsheetId = "1fwpQNO9ajJxneCR3Hi3kr5SL6pFDPLb1LkLEpIqjZ4Q";  // TODO: Update placeholder value.

        // The A1 notation of the values to retrieve.
        string range = "A2:Z1000";  // TODO: Update placeholder value.

        // How values should be represented in the output.
        // The default render option is ValueRenderOption.FORMATTED_VALUE.
        SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;  // TODO: Update placeholder value.

        // How dates, times, and durations should be represented in the output.
        // This is ignored if value_render_option is
        // FORMATTED_VALUE.
        // The default dateTime render option is [DateTimeRenderOption.SERIAL_NUMBER].
        ///SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum dateTimeRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum)0;  // TODO: Update placeholder value.

        SpreadsheetsResource.ValuesResource.GetRequest request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        request.ValueRenderOption = valueRenderOption;

        try
        {
            Data.ValueRange response = request.Execute();
            
            string s = JsonConvert.SerializeObject(response); // Capturing the data sheet
            //Debug.Log($"File: {s}"); //used to see what the data looks like when coming in

            string[] removeJunkArray = s.Split(':'); // Splitting all the parts into what is most important

            s = removeJunkArray[4]; // #5 in this array actually contains all the data ,Also trimming the first two characters off

            s = s.Remove(s.Length - 9, 9); // Trimming the last 9 characters off

            removeJunkArray = s.Split("]"); //This is splitting the table into the rows

            List<string[]> ar = new List<string[]>(); 

            foreach (string i in removeJunkArray) //Creating the grid by which the sheet is layed out.
            {
                s = i.Remove(0, 2);
                string[] newArray = s.Split(",");
                string[] actualArray = new string[newArray.Length];
                for (int j = 0; j < newArray.Length; j++)
                {
                    s = newArray[j].Trim('"');
                    actualArray[j] = s;
                }
                ar.Add(actualArray);
            }

            return ar;
        }
        catch(Exception e)//System.Net.WebException e)
        {
            Debug.Log(e.Message + ", The game is offline and needs to build from memory.");
        }

        return null;
    }
}