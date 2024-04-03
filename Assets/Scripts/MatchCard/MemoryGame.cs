using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Minigames
{
    [Serializable]
    public class Match
    {
        [SerializeField] private MemoryCard cardToMatch;
        public MemoryCard CardToMatch
        {
            get { return cardToMatch; }
            set { cardToMatch = value; }
        }

        [SerializeField] private MemoryCard duplicateCardToMatch;
        public MemoryCard DuplicateCardToMatch
        {
            get { return duplicateCardToMatch; }
            set { duplicateCardToMatch = value; }
        }

        private bool isMatchedUp;
        public bool IsMatchedUp => isMatchedUp;

        public bool MatchUp(MemoryCard currentSelectedCard)
        {
            if (currentSelectedCard == cardToMatch || currentSelectedCard == duplicateCardToMatch)
            {
                isMatchedUp = true;
                cardToMatch.IsDone = true;
                duplicateCardToMatch.IsDone = true;
                return true;
            }
            return false;
        }
    }

    public class MemoryGame : MonoBehaviour
    {
        [SerializeField] private GameObject[] memoryCardPrefabs;

        private MemoryCard currentSelectedCard = null;
        private MemoryCard previousSelectedCard = null;

        [Tooltip("Ideal vector3 is (0,0,0) in local position")]
        [SerializeField] private Transform cardSpawnAnchor;

        [Tooltip("X and Y should multiply and equal to double the number of prefabs")][SerializeField] private int gridXSize, gridYSize;
        private int point = 0, copySpawned = 0, index = 0;

        [Tooltip("Determines the gap size between each card in x axis")][SerializeField] private float gapXLength;
        [Tooltip("Determines the gap size between each card in y axis")] [SerializeField] private float gapYLength;

        public Action OnMemoryGameSuccess;

        [HideInInspector] [SerializeField] private List<Match> Matches;

        //For shuffle
        [HideInInspector] [SerializeField] private List<GameObject> gameObjectList = new List<GameObject>();
        [HideInInspector] [SerializeField] private List<Vector3> cardPositions = new List<Vector3>();

        private void Start()
        {
            if (cardSpawnAnchor.childCount <= 0)
            {
                print("Please generate cards first");
                return;
            }

            foreach (Transform child in cardSpawnAnchor)
            {
                if (!child.GetComponentInChildren<MemoryCard>()) { return; }

                MemoryCard instance = child.GetComponentInChildren<MemoryCard>();
                instance.StartCoroutine(instance.FlipClose());

                instance.OnSelectedCard += CheckIfMatchUp;
                instance.OnDeselect += ResetToNull;
            }

            ShuffleCards();
        }

        public void GenerateGame()
        {
            if (cardSpawnAnchor.childCount > 0)
            {
                print($"Please destroy all copies in the {cardSpawnAnchor} first");
                return;
            }
            GenerationImplementation(gapXLength, gapYLength);
        }

        public void ResetAll()
        {
            index = 0;
            copySpawned = 0;
            while (cardSpawnAnchor.childCount > 0)
            {
                DestroyImmediate(cardSpawnAnchor.GetChild(0).gameObject);
            }

            if (Matches.Count > 0)
            {
                Matches.Clear();
            }
            if (gameObjectList.Count > 0)
            {
                gameObjectList.Clear();
            }
            if (cardPositions.Count > 0)
            {
                cardPositions.Clear();
            }

        }

        private void CheckIfMatchUp(MemoryCard currentCard)
        {
            previousSelectedCard = currentSelectedCard;
            currentSelectedCard = currentCard;

            if (previousSelectedCard == null) return;

            for (int i = 0; i < Matches.Count; i++)
            {
                if (Matches[i].IsMatchedUp) { continue; }

                if (Matches[i].CardToMatch == previousSelectedCard || Matches[i].DuplicateCardToMatch == previousSelectedCard)
                {
                    SetCards(false);

                    if (Matches[i].MatchUp(currentSelectedCard))
                    {
                        AddPoint();
                        ResetToNull();
                    }
                    else
                    {
                        StartCoroutine(ResetCards());
                        //Debug.LogError("Reset");
                    }
                }
            }
        }

        private void SetCards(bool isEnabled)
        {
            for (int i = 0; i < Matches.Count; i++)
            {
                if (Matches[i].IsMatchedUp) { continue; }

                Matches[i].CardToMatch.IsEnabled = isEnabled;
                Matches[i].DuplicateCardToMatch.IsEnabled = isEnabled;

                if (isEnabled) { continue; }

                if (Matches[i].CardToMatch != previousSelectedCard || Matches[i].CardToMatch != currentSelectedCard)
                {
                    Matches[i].CardToMatch.SetOutline(false);
                }
                if (Matches[i].DuplicateCardToMatch != previousSelectedCard || Matches[i].DuplicateCardToMatch != currentSelectedCard)
                {
                    Matches[i].DuplicateCardToMatch.SetOutline(false);
                }
            }
        }

        private IEnumerator ResetCards()
        {
            if (previousSelectedCard && currentSelectedCard)
            {
                yield return new WaitForSeconds(.4f);
                previousSelectedCard.ResetFlip();
                currentSelectedCard.ResetFlip();
            }
        }

        private void ResetToNull()
        {
            StopAllCoroutines();
            previousSelectedCard = null;
            currentSelectedCard = null;
            SetCards(true);
        }

        private void AddPoint()
        {
            point++;
            if (point >= Matches.Count)
            {
                OnMemoryGameSuccess?.Invoke();
                //Debug.LogError("Succuess");
            }
        }

        private void AssignToLists(GameObject obj, int matchIndex)
        {
            MemoryCard card = obj.GetComponentInChildren<MemoryCard>();

            if (Matches[matchIndex].CardToMatch == null)
            {
                Matches[matchIndex].CardToMatch = card;
            }
            else
            {
                Matches[matchIndex].DuplicateCardToMatch = card;
            }
            gameObjectList.Add(obj);
            cardPositions.Add(obj.transform.position);
        }

        public void ShuffleCards()
        {
            for (int i = 0; i < gameObjectList.Count; i++)
            {
                GameObject tempObject = gameObjectList[i];
                int randomIndex = UnityEngine.Random.Range(i, gameObjectList.Count);
                gameObjectList[i] = gameObjectList[randomIndex];
                gameObjectList[randomIndex] = tempObject;
            }

            if (cardPositions.Count != gameObjectList.Count) { return; }

            for (int j = 0; j < cardPositions.Count; j++)
            {
                gameObjectList[j].transform.position = cardPositions[j];
            }
        }

        //Instantiate with different cards; each card has two copies to match with each other
        private void GenerationImplementation(float _xAxis, float _yAxis)
        {
            for (int m = 0; m < memoryCardPrefabs.Length; m++)
            {
                Match match = new Match();
                Matches.Add(match);
            }
            for (int i = 0; i < gridXSize; i++)
            {
                for (int j = 0; j < gridYSize; j++)
                {
                    GameObject obj = Instantiate(memoryCardPrefabs[index].gameObject, cardSpawnAnchor);
                    obj.transform.position = new Vector3(_xAxis * i, _yAxis * j, 0) + cardSpawnAnchor.position;
                    //Debug.LogError(obj.transform.position + " i" + i + " j" + j);
                    obj.transform.rotation = Quaternion.identity;

                    MemoryCard instance = obj.GetComponentInChildren<MemoryCard>();

                    AssignToLists(obj, index);

                    if (!CheckCopyNeeded(instance))
                    {
                        index++;
                        copySpawned = 0;
                    }

                }
            }
            ShuffleCards();
        }

        private bool CheckCopyNeeded(MemoryCard currentCard)
        {
            if (copySpawned < currentCard.CopyNeeded - 1)
            {
                copySpawned++;
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(MemoryGame))]
        public class MemoryGame_Editor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector(); // for other non-HideInInspector fields

                MemoryGame script = (MemoryGame)target;

                if (GUILayout.Button("Generate cards in scene"))
                {
                    script.GenerateGame();
                }
                if (GUILayout.Button("Reset"))
                {
                    script.ResetAll();
                }
            }
        }
#endif
    }
}

