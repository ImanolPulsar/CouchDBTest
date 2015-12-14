using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Couchbase.Lite;
using Couchbase.Lite.Unity;
using Couchbase.Lite.Auth;

public class DatabaseController : MonoBehaviour {

    const string RACER_DATABASE_NAME = "race_records";

    Database database;
    Manager dbManage;

    Replication _pusher;
    Replication _puller;

    Uri replicationUrl = new Uri("http://127.0.0.1:5984/racer_scores");

    void Start() {
        //string path = Application.persistentDataPath;

        //dbManage = new Manager(new DirectoryInfo(path), new ManagerOptions { CallbackScheduler = UnityMainThreadScheduler.TaskScheduler });
        //database = dbManage.GetDatabase("arcade_records");

        string path = Application.persistentDataPath;
        dbManage = new Manager(new DirectoryInfo(path), new ManagerOptions { CallbackScheduler = UnityMainThreadScheduler.TaskScheduler });
        database = dbManage.GetDatabase(RACER_DATABASE_NAME);

        //CreateDatabase();
    }

    void CreateDatabase() {

        var document = database.CreateDocument();
        var properties = new Dictionary<string, object>() {
            {"type", "player"},
            {"name", "Imanolea"},
            {"record", 245000}
        };
        var rev = document.PutProperties(properties);
        Debug.Assert(rev != null);
    }

    void Update() {
        //if (Input.GetKeyDown(KeyCode.H) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
        //    foreach (string result in TopTenScores()) {
        //        Debug.Log(result);
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.J) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
        //    SyncDatabase();
        //}
    }

    public void SetNewScore(string newRacerName, string newRacerTime) {

        Document document = database.CreateDocument();
        Dictionary<string, object> properties = new Dictionary<string, object>() {
            {"$doctype", "racer"},
            {"name", newRacerName},
            {"time", newRacerTime}
        };
        SavedRevision rev = document.PutProperties(properties);
        Debug.Assert(rev != null);
    }

    void SetUpViews() {

        View timesView = database.GetView("racers_time");
        timesView.Delete();
        timesView = database.GetView("racers_time");

        bool success = timesView.SetMap((doc, emit) => {
            object key;
            bool hasType = doc.TryGetValue("$doctype", out key);
            Debug.Assert(hasType);
            if (hasType && key.Equals("racer")) {
                emit(doc["time"], doc["name"]);
            }
        }, "1");
        //Debug.Assert(success);

        Debug.Log("Vistas creadas");
    }

    public List<string> TopTenScores() {

        SyncDatabase();

        SetUpViews();

        return QueryTopTenScores();
    }

    public void SyncDatabase() {

        Debug.Log("Sincronización con CouchDB");

        _pusher = database.CreatePushReplication(replicationUrl);
        _puller = database.CreatePullReplication(replicationUrl);
        var auth = AuthenticatorFactory.CreateBasicAuthenticator("PulsarAdmin", "1234");
        _pusher.Authenticator = auth;
        _puller.Authenticator = auth;
        _pusher.Continuous = true;
        _pusher.Changed += (sender, e) => {
            Debug.Log ("Pusher: " + _pusher.LastError == null ? "Okay" : _pusher.LastError.Message + _pusher.LastError.ToString());
        };
        _puller.Continuous = true;
        _puller.Changed += (sender, e) => {
            Debug.Log("Puller: " + _puller.LastError == null ? "Okay" : _puller.LastError.Message + _pusher.LastError.ToString());
        };

        _pusher.Start();
        _puller.Start();
    }

    List<string> QueryTopTenScores() {

        Debug.Log("Top 10 de tiempos");

        List<string> topTenScores = new List<string>();

        Query _query = database.GetExistingView("racers_time").CreateQuery();
        Debug.Assert(_query != null);
        _query.Descending = false;
        _query.Limit = 10;
        QueryEnumerator rows = _query.Run();
        Debug.Assert(rows != null);
        foreach (QueryRow row in rows) {
            var name = row.Value;
            var time = row.Key;
            topTenScores.Add(name + ";" + time);
        }

        return topTenScores;
    }

    void OnApplicationQuit() {
        dbManage.Close();
        _puller.Stop();
        _pusher.Stop();
    }

}
