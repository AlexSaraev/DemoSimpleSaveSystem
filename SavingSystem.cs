using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Control;
using UnityEngine;

namespace Saving
{
    public class SavingSystem : MonoBehaviour
    {
        // === PUBLIC METHODS ===
        public void Save(string saveFile)
        {
            Dictionary<string, object> state = GetFile(saveFile);
            CaptureState(state);
            SaveFile(saveFile, state);
        }

        public void Save(string saveFile, string key, object value, bool captureStates = true)
        {
            Dictionary<string, object> state = GetFile(saveFile);
            if (captureStates) CaptureState(state);
            state[key] = value;
            SaveFile(saveFile, state);
        }

        public IEnumerator LoadScene(int sceneBuildIndex, string saveFile)
        {
            Dictionary<string, object> state = GetFile(saveFile);
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex);
            RestoreState(state);
        }

        public void Load(string saveFile)
        {
            RestoreState(GetFile(saveFile));
        }

        public void Delete(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Delete from " + path);
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        public Dictionary<string, object> GetFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path))
            {
                return new Dictionary<string, object>();
            }
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        // === PRIVATE METHODS ===
        void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Saving to " + path);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                state[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
            }

            state["lastSceneBuildIndex"] = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        }

        void RestoreState(Dictionary<string, object> state)
        {
            // 1
            PlaceCharacters(state);
            // 2 
            RestoreAllStates(state);
        }

        void PlaceCharacters(Dictionary<string, object> state)
        {
            var spawnerEnity = GetSpawnerEntity();

            if (spawnerEnity != null)
            {
                var entityID = spawnerEnity.GetUniqueIdentifier();

                if (state.ContainsKey(entityID))
                {
                    spawnerEnity.RestoreState(state[entityID]);
                }
            }
        }

        void RestoreAllStates(Dictionary<string, object> state)
        {
            var spawnerEnityID = GetSpawnerEntity()?.GetUniqueIdentifier() ?? "";

            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();

                if (id == spawnerEnityID) continue;
                if (state.ContainsKey(id)) saveable.RestoreState(state[id]);
            }
        }

        SaveableEntity GetSpawnerEntity()
        {
            var spawner = FindObjectOfType<Spawner>();

            if (spawner != null)
                return spawner.GetComponent<SaveableEntity>();
            else
                return null;
        }

        string GetPathFromSaveFile(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
        }
    }
}