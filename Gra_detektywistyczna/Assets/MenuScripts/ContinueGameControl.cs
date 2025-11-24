using DTOModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Assets.MenuScripts
{
    public class ContinueGameControl : MonoBehaviour
    {    
        [SerializeField] public Transform gamesContainer;
        [SerializeField] public GameObject gameTilePrefab;

        private async void Awake()
        {
            foreach (Transform child in gamesContainer)
            {
                Destroy(child.gameObject);
            }

            GamesToContinueDTO gamesToContinue = await DialogueEngineManager.Instance.GetGamesToContinueAsync();         
           
            var sortedGames = gamesToContinue.GamesToContinue.OrderByDescending(g => g.LastSaveDate).ToList();  

            foreach (CreatedGameDTO game in sortedGames)
            {
                GameObject tile = Instantiate(gameTilePrefab, gamesContainer);

                tile.transform.Find("Title").GetComponent<TMP_Text>().text = $"Tytuł: {game.Title}";
                tile.transform.Find("CurrentSceneNumber").GetComponent<TMP_Text>().text = $"Scena: {game.CurrentSceneNumber}/{game.MaxSceneNumber}";
                DateTime displayDate = DateTime.Parse(game.LastSaveDate);
                string data = displayDate.Year + "." + displayDate.Month + "." + displayDate.Day;
                string godzina = displayDate.Hour + "." + displayDate.Minute;
                tile.transform.Find("LastSaveDate").GetComponent<TMP_Text>().text = $"Ostatni zapis: {data} godz. {godzina}";

                Button btn = tile.GetComponent<Button>();
                btn.onClick.AddListener(() => OnGameTileClicked(game));
            }
        }

        // Pusta metoda pod kliknięcie kafelka
        private void OnGameTileClicked(CreatedGameDTO game)
        {
        }















        //private async void Awake()
        //{
        //GamesToContinueDTO gamesToContinue = await DialogueEngineManager.Instance.GetGamesToContinueAsync();
        //foreach (CreatedGameDTO game in gamesToContinue.GamesToContinue)
        //{
        //Debug.Log($"{game.Title}");
        //}
        //}
    }
}
