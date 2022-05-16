using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class StorageManager : MonoBehaviour
{
    [Header("DataToCarryBetweenScenes")]
    //public int numberOfDrones;
    //public float pointDistance;

    [Header("StorageStructure")]
    List<string> dataPaths = new List<string>();
    public string simulationPath;

    private void Start()
    {
        //Genereer alle locaties van de bestanden
        dataPaths = GetDataPaths();
    }

    //Genereer alle locaties van de bestanden
    //Deze functie wordt ook getriggerd uit de Initialiser
    public List<string> GetDataPaths()
    {
        //Application.persistentDataPath is een pad dat eigen is aan het programma en nooit verandert, daarom is het dus handig dit te gebruiken als opslag
        //Onder Application.persistentDataPath wordt een map save gemaakt met twee mappen, accounts en events, de events map krijgt ook nog twee mappen, race en training
        simulationPath = Application.persistentDataPath + "/SimulationLogs";
        dataPaths.Add(simulationPath);
        return dataPaths;
    }

    //Sla een lijst van floats op in een gegeven locatie, als er al een bestand is in deze locatie wordt er gewoon overheen geschreven
    public void SaveAndReplace(string file_location, List<float> dataList)
    {
        using (FileStream fs = File.Create(file_location))
        {
            string file_string = "";

            //De floats uit de dataList worden achter elkaar geplaatst in een string met slashes ertussen.
            foreach (float data_point in dataList)
            {
                file_string = file_string + "/" + data_point.ToString();
            }

            //Schrijf deze string naar het bestand en sluit het daarna
            //Elk teken in de string wordt omgezet naar een byte aan de hand van de UTF8Encoding zodat het naar het tekstbestand kan worden geschreven in die vorm
            byte[] contentBytes = new UTF8Encoding(true).GetBytes(file_string);

            //Schrijf nu deze Array van bytes naar het bestand, beginnend op de eerste plaats (offset 0)
            fs.Write(contentBytes, 0, contentBytes.Length);
            fs.Close();
        }
    }

    //Voeg een lijst van floats toe aan een al bestaand bestand
    public void SaveAndAdd(string file_location, List<float> dataList)
    {
        //Lees de data die er al in het bestand staat
        List<float> priorData = new List<float>();
        priorData = Load(file_location);

        //Voeg oude en nieuwe data samen
        List<float> joinedData = new List<float>();
        joinedData.AddRange(priorData); joinedData.AddRange(dataList);

        //Sla deze data op op de 'normale' manier met vervanging
        SaveAndReplace(file_location, joinedData);
    }

    //Lees een bestand
    public List<float> Load(string file_location)
    {

        string text = "";

        //Lees elke byte en zet deze terug om in een string
        using (FileStream fs = File.OpenRead(file_location))
        {

            byte[] b = new byte[1024];
            UTF8Encoding temp = new UTF8Encoding(true);
            while (fs.Read(b, 0, b.Length) > 0)
            {
                text = text + temp.GetString(b);
            }

        }

        //Zet de strings terug om in een float

        List<float> dataList = new List<float>();
        //Elke nieuwe lijn wordt in het tekstbestand aangeduid met een /, deze worden eeruit gehaald aan de hand van de split functie
        string[] dataLines = text.Split('/');      //"512,1230
        foreach (string dataLine in dataLines)
        {
            //Zet de string om in een float en voeg deze toe aan de lijst met floats
            try
            {
                dataList.Add(float.Parse(dataLine));
            }
            catch { }
            /*
            List<float> dataLijst = new List<float>();
            string[] dataPoints = dataLine.Split(',');

            foreach (string dataPoint in dataPoints)
            {
                dataLijst.Add(float.Parse(dataPoint));
            }
            puntenLijst.Add(dataLijst);
            */

        }
        return (dataList);
    }


    //Sla een training op
    public void SaveSimulation(string simName, float simulationTime, float simulatedTime, float numberOfDrones)
    {
        //Creer de datum en tijd in het "MMddyyyy_HHmmss" formaat
        string dateTime = DateTime.Now.ToString("MMddyyyy_HHmmss");

        //Maak een lijst van floats van alle argumenten
        List<float> toSaveFloats = new List<float>();
        toSaveFloats.Add(simulationTime); toSaveFloats.Add(simulatedTime); toSaveFloats.Add(numberOfDrones);

        //Maak de locatie waar dit bestand zal worden opgeslagen
        string saveLocation = simulationPath + "/" + simName + "," + dateTime + "," + ".txt";

        //Sla het bestand hier op
        SaveAndReplace(saveLocation, toSaveFloats);
    }
}
