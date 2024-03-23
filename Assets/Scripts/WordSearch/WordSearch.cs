using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SpawnPlacement
{
    VerticalPlacement,
    HorizontalPlacement
}

[Serializable]
public class Word
{
    [HideInInspector] [SerializeField] private Letter[] correctOrder;

    public Letter[] CorrectOrder => correctOrder;

    [HideInInspector] [SerializeField] private List<Letter> spawnedList;
    public List<Letter> SpawnedList => spawnedList;

    // [x,] 
    [HideInInspector] [SerializeField] private int wordRowStart;
    public int WordRowStart
    {
        get { return wordRowStart; }
        set { wordRowStart = value; }
    }
    // [,y]
    [HideInInspector] [SerializeField] private int wordColumnStart;
    public int WordColumnStart
    {
        get { return wordColumnStart; }
        set { wordColumnStart = value; }
    }

    [SerializeField] private SpawnPlacement spawnPlacement;
    public SpawnPlacement SpawnPlacement => spawnPlacement;

    [SerializeField] private int puzzleNumber = 2;

    public int PuzzleNumber => puzzleNumber;
}

public class WordSearch : MonoBehaviour
{
    [SerializeField] private List<Word> words;

    [SerializeField] private List<GameObject> letterPrefabs;

    [HideInInspector] [SerializeField] private List<Letter> totalSpawnedList;

    [HideInInspector] [SerializeField] private List<Letter> tempRelativeOptions;

    //These must be the same as lettersRowColumn
    [SerializeField] private int gridXSize;
    [SerializeField] private int gridZSize;

    [SerializeField] private Transform spawnParent;

    private int point = 0;

    //These sizes must be changed manually in the script if you want to generate other sizes of word scramble
    [SerializeField] private Letter[,] lettersRowColumn = new Letter[12, 12];

    [HideInInspector] [SerializeField] private Letter currentSelectedLetter = null;
    [HideInInspector] [SerializeField] private Letter previousSelectedLetter = null;

    public Action OnWordScrambleFinished;

    private void Start()
    {
        if (spawnParent.childCount <= 0)
        {
            print("Please generate letters first");
            return;
        }

        foreach (Transform child in spawnParent)
        {
            if (!child.GetComponent<Letter>()) { return; }

            Letter instance = child.GetComponent<Letter>();

            instance.OnSelectedLetter += CheckIfCorrectlySwapped;
            instance.OnDeselect += ResetToNull;
        }
    }

    private void ShowOnlyNearbyLetters(Letter currentLetter)
    {
        GetAvailableOptions(currentLetter);

        foreach (Transform child in spawnParent)
        {
            Letter instance = child.GetComponent<Letter>();

            if (tempRelativeOptions.Contains(instance))
            {
                //instance.OutlineController.ChangeColour(Color.yellow);
                //instance.OutlineController.EnableColouredOutline();
                instance.IsShownAsOption = true;
            }
            else
            {
                if (instance == currentLetter) { continue; }

                if (instance.IsDone) { continue; }

                instance.IsEnabled = false;
                //instance.OutlineController.DisableColouredOutline();
            }
        }
    }

    private void GetAvailableOptions(Letter currentLetter)
    {
        int z;

        if (currentLetter.ColumnAxis + 1 < gridZSize)
        {
            z = currentLetter.ColumnAxis + 1;
            Letter option = GetThatLetter(currentLetter.RowAxis, z);

            if (option.IsDone) { option = null; }
            tempRelativeOptions.Add(option);
        }
        if (currentLetter.ColumnAxis - 1 >= 0)
        {
            z = currentLetter.ColumnAxis - 1;
            Letter option = GetThatLetter(currentLetter.RowAxis, z);

            if (option.IsDone) { option = null; }
            tempRelativeOptions.Add(option);
        }

        int x;

        if (currentLetter.RowAxis + 1 < gridXSize)
        {
            x = currentLetter.RowAxis + 1;
            Letter option = GetThatLetter(x, currentLetter.ColumnAxis);

            if (option.IsDone) { option = null; }
            tempRelativeOptions.Add(option);
        }
        if (currentLetter.RowAxis - 1 >= 0)
        {
            x = currentLetter.RowAxis - 1;
            Letter option = GetThatLetter(x, currentLetter.ColumnAxis);

            if (option.IsDone) { option = null; }
            tempRelativeOptions.Add(option);
        }
    }

    private Letter GetThatLetter(int row, int column)
    {
        foreach (Transform child in spawnParent)
        {
            Letter instance = child.GetComponent<Letter>();

            if (instance.RowAxis == row && instance.ColumnAxis == column)
            {
                return instance;
            }
        }
        return null;
    }

    private void CheckIfCorrectlySwapped(Letter currentLetter)
    {
        previousSelectedLetter = currentSelectedLetter;
        currentSelectedLetter = currentLetter;

        if (previousSelectedLetter == null) { ShowOnlyNearbyLetters(currentLetter); return; }

        setLetters(false);

        if (previousSelectedLetter.SwappedCard == currentSelectedLetter && currentSelectedLetter.SwappedCard == previousSelectedLetter)
        {
            addPoint();
            previousSelectedLetter.Swap();
            currentSelectedLetter.Swap();

            //Swapped ints of row and column
            int originalRow = previousSelectedLetter.RowAxis;
            int originalColumn = previousSelectedLetter.ColumnAxis;

            previousSelectedLetter.RowAxis = currentSelectedLetter.RowAxis;
            previousSelectedLetter.ColumnAxis = currentSelectedLetter.ColumnAxis;
            currentSelectedLetter.RowAxis = originalRow;
            currentSelectedLetter.ColumnAxis = originalColumn;

            highLightCompletedPuzzle(currentSelectedLetter, previousSelectedLetter);

            ResetToNull();
        }
        else
        {
            StartCoroutine(ResetLetters());
        }

        foreach (Transform child in spawnParent)
        {
            if (!child.GetComponent<Letter>()) { return; }

            Letter instance = child.GetComponent<Letter>();

            if (instance.IsShownAsOption)
            {
                instance.IsShownAsOption = false;
            }
        }
    }
    private void setLetters(bool isEnabled)
    {
        foreach (Transform child in spawnParent)
        {
            if (!child.GetComponent<Letter>()) { return; }

            Letter instance = child.GetComponent<Letter>();

            if (instance.IsDone) { continue; }

            if (instance == previousSelectedLetter || instance == currentSelectedLetter)
            {
                continue;
            }
            instance.IsEnabled = isEnabled;

            if (isEnabled) { continue; }

            //instance.OutlineController.DisableColouredOutline();
        }
    }

    private void highLightCompletedPuzzle(Letter currentLetter, Letter previousLetter)
    {
        for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
        {
            if (words[wordIndex].SpawnedList.Contains(previousLetter) || words[wordIndex].SpawnedList.Contains(currentLetter))
            {
                for (int i = 0; i < words[wordIndex].SpawnedList.Count; i++)
                {
                    Letter letter = words[wordIndex].SpawnedList[i];

                    //Reset the swapped letter
                    if (letter == previousLetter)
                    {
                        previousLetter.SwappedCard.ResetLetter();
                    }
                    else
                    {
                        currentLetter.SwappedCard.ResetLetter();
                    }
                    if (letter.IsDone) { continue; }

                    letter.IsDone = true;
                    //letter.OutlineController.ChangeColour(Color.green);
                    //letter.OutlineController.EnableColouredOutline();
                }
            }
        }
    }

    private void addPoint()
    {
        point++;
        if (point >= words.Count)
        {
            OnWordScrambleFinished?.Invoke();
            //Debug.LogError("Succuess");
        }
    }

    private IEnumerator ResetLetters()
    {
        if (previousSelectedLetter != null && currentSelectedLetter != null)
        {
            previousSelectedLetter.IncorrectSwap();
            currentSelectedLetter.IncorrectSwap();
            yield return new WaitForSeconds(.4f);
            previousSelectedLetter.ResetLetter();
            currentSelectedLetter.ResetLetter();

            ResetToNull();
        }
    }

    private void ResetToNull()
    {
        StopAllCoroutines();
        previousSelectedLetter = null;
        currentSelectedLetter = null;

        tempRelativeOptions.Clear();
        setLetters(false);
        setLetters(true);
    }
    private void GenerateLetters()
    {
        if (spawnParent.childCount > 0)
        {
            print($"Please destroy all copies in the {spawnParent} first");
            return;
        }
        GenerationImplementation(.06f, .06f);
    }

    private void GenerationImplementation(float _xAxis, float _zAxis)
    {
        int wordIndex = 0;

        for (int i = 0; i < words.Count; i++)
        {
            wordIndex = i;
            spawnWord(wordIndex, _xAxis, _zAxis);
        }

        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {
                if (lettersRowColumn[x, z] != null) { continue; }

                int index = UnityEngine.Random.Range(0, letterPrefabs.Count);
                GameObject obj = Instantiate(letterPrefabs[index].gameObject, spawnParent);
                obj.transform.localPosition = new Vector3(-_xAxis * x, 0, _zAxis * z) + spawnParent.localPosition;
                obj.transform.localRotation = Quaternion.identity;

                //AssignToLists(obj, index);

                lettersRowColumn[x, z] = obj.GetComponent<Letter>();
                lettersRowColumn[x, z].RowAxis = x;
                lettersRowColumn[x, z].ColumnAxis = z;
            }
        }

        formPuzzles();
    }

    private void formPuzzles()
    {
        for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
        {
            if (words[wordIndex].SpawnedList.Count <= 0)
            {
                print($"The word list has no letters {words[wordIndex].SpawnedList}");
                return;
            }
            int puzzleFormed = 0;
            for (int letterIndex = 0; letterIndex < words[wordIndex].SpawnedList.Count; letterIndex++)
            {
                bool shouldFormPuzzle = UnityEngine.Random.value > 0.7f;

                if (!shouldFormPuzzle)
                {
                    if (puzzleFormed <= 0 && letterIndex >= words[wordIndex].SpawnedList.Count - 1)
                    {
                        letterIndex = 0;
                    }
                    continue;
                }

                if (puzzleFormed >= words[wordIndex].PuzzleNumber)
                {
                    continue;
                }
                int x = words[wordIndex].SpawnedList[letterIndex].RowAxis;
                int z = words[wordIndex].SpawnedList[letterIndex].ColumnAxis;

                Letter nearbyLetter = GetAvailableNearbyLetter(x, z, wordIndex);

                if (nearbyLetter == null)
                {
                    continue;
                }
                nearbyLetter.IsLocked = true;

                //These are used to swap them back
                words[wordIndex].SpawnedList[letterIndex].SwappedCard = nearbyLetter;
                nearbyLetter.SwappedCard = words[wordIndex].SpawnedList[letterIndex];

                //Swapped ints of row and column
                int originalRow = words[wordIndex].SpawnedList[letterIndex].RowAxis;
                int originalColumn = words[wordIndex].SpawnedList[letterIndex].ColumnAxis;

                words[wordIndex].SpawnedList[letterIndex].RowAxis = nearbyLetter.RowAxis;
                words[wordIndex].SpawnedList[letterIndex].ColumnAxis = nearbyLetter.ColumnAxis;
                nearbyLetter.RowAxis = originalRow;
                nearbyLetter.ColumnAxis = originalColumn;

                //Cached transform values
                Vector3 letterLocalPos = words[wordIndex].SpawnedList[letterIndex].transform.localPosition;
                Quaternion letterLocalRot = words[wordIndex].SpawnedList[letterIndex].transform.localRotation;

                //Switched letter pos and rot to swapped letter pos and rot
                words[wordIndex].SpawnedList[letterIndex].transform.localPosition = nearbyLetter.transform.localPosition;
                words[wordIndex].SpawnedList[letterIndex].transform.localRotation = nearbyLetter.transform.localRotation;

                //opposite to above 
                words[wordIndex].SpawnedList[letterIndex].SwappedCard.transform.localPosition = letterLocalPos;
                words[wordIndex].SpawnedList[letterIndex].SwappedCard.transform.localRotation = letterLocalRot;

                puzzleFormed++;
            }
        }
    }

    private Letter GetAvailableNearbyLetter(int x, int z, int wordIndex)
    {
        Letter letterInQuestion = lettersRowColumn[x, z];

        if (words[wordIndex].SpawnPlacement == SpawnPlacement.HorizontalPlacement)
        {
            //Get the relative letter below, must not be in spawned answer letters, must not be locked, must not be same alphabet
            if (z + 1 < gridZSize && !totalSpawnedList.Contains(lettersRowColumn[x, z + 1]) && lettersRowColumn[x, z + 1].IsLocked == false &&
                letterInQuestion.ThisAlphabet != lettersRowColumn[x, z + 1].ThisAlphabet)
            {
                z = z + 1;
                return lettersRowColumn[x, z];
            }
            //Get the relative letter above
            else if (z - 1 >= 0 && !totalSpawnedList.Contains(lettersRowColumn[x, z - 1]) && lettersRowColumn[x, z - 1].IsLocked == false &&
                letterInQuestion.ThisAlphabet != lettersRowColumn[x, z - 1].ThisAlphabet)
            {
                z = z - 1;
                return lettersRowColumn[x, z];
            }
        }
        else
        {
            //Get the right letter
            if (x + 1 < gridXSize && !totalSpawnedList.Contains(lettersRowColumn[x + 1, z]) && lettersRowColumn[x + 1, z].IsLocked == false &&
                letterInQuestion.ThisAlphabet != lettersRowColumn[x + 1, z].ThisAlphabet)
            {
                x = x + 1;
                return lettersRowColumn[x, z];
            }
            //Get the left letter
            else if (x - 1 >= 0 && !totalSpawnedList.Contains(lettersRowColumn[x - 1, z]) && lettersRowColumn[x - 1, z].IsLocked == false &&
                letterInQuestion.ThisAlphabet != lettersRowColumn[x - 1, z].ThisAlphabet)
            {
                x = x - 1;
                return lettersRowColumn[x, z];
            }
        }
        //if no letter available, return null
        return null;
    }

    private void spawnWord(int wordIndex, float _xAxis, float _zAxis)
    {
        var chosenCoordinates = GetTwoRandomPoints(wordIndex);

        if (chosenCoordinates == null)
        {
            spawnWord(wordIndex, _xAxis, _zAxis);
            return;
        }
        int x = chosenCoordinates.Item1;
        int z = chosenCoordinates.Item2;

        //Get two random points in 2d array, x and z. Then start instantiating prefabs into the array.
        // + 1 depending on the placement type. 
        for (int i = 0; i < words[wordIndex].CorrectOrder.Length; i++)
        {
            GameObject obj = Instantiate(words[wordIndex].CorrectOrder[i].gameObject, spawnParent);
            obj.transform.localPosition = new Vector3(-_xAxis * x, 0, _zAxis * z) + spawnParent.localPosition;
            obj.transform.localRotation = Quaternion.identity;

            lettersRowColumn[x, z] = obj.GetComponent<Letter>();
            lettersRowColumn[x, z].RowAxis = x;
            lettersRowColumn[x, z].ColumnAxis = z;
            words[wordIndex].SpawnedList.Add(lettersRowColumn[x, z]);
            totalSpawnedList.Add(lettersRowColumn[x, z]);

            if (words[wordIndex].SpawnPlacement == SpawnPlacement.HorizontalPlacement)
            {
                x++;
            }
            else
            {
                z++;
            }
        }

        //Save the random points in the word, int rowstart, columnstart
        words[wordIndex].WordRowStart = chosenCoordinates.Item1;
        words[wordIndex].WordColumnStart = chosenCoordinates.Item2;
    }

    private Tuple<int, int, bool> GetTwoRandomPoints(int index)
    {
        SpawnPlacement currentPlacement = words[index].SpawnPlacement;
        int x, z;
        bool isReverse = UnityEngine.Random.value > 0.5f;
        switch (currentPlacement)
        {
            case SpawnPlacement.HorizontalPlacement:
                x = UnityEngine.Random.Range(0, (gridXSize - words[index].CorrectOrder.Length) + 1);
                z = UnityEngine.Random.Range(0, gridZSize);

                if (!isAxisEmpty(z, x, index))
                {
                    return null;
                }
                return new Tuple<int, int, bool>(x, z, isReverse);
            case SpawnPlacement.VerticalPlacement:
                x = UnityEngine.Random.Range(0, gridXSize);
                z = UnityEngine.Random.Range(0, (gridZSize - words[index].CorrectOrder.Length) + 1);

                if (!isAxisEmpty(x, z, index))
                {
                    return null;
                }
                return new Tuple<int, int, bool>(x, z, isReverse);
        }
        return null;
    }

    private bool isAxisEmpty(int axisToNotCheck, int axisToCheck, int index)
    {
        int count = 0;
        SpawnPlacement currentPlacement = words[index].SpawnPlacement;
        for (int axisChecking = axisToCheck; axisChecking < axisToCheck + (words[index].CorrectOrder.Length); axisChecking++)
        {
            if (currentPlacement == SpawnPlacement.HorizontalPlacement)
            {
                if (lettersRowColumn[axisChecking, axisToNotCheck] != null)
                {
                    count++;
                }
                if (count > 0) { return false; }

            }
            else
            {
                if (lettersRowColumn[axisToNotCheck, axisChecking] != null)
                {
                    count++;
                }
                if (count > 0) { return false; }
            }
        }
        return true;
    }
    private void ResetAll()
    {
        while (spawnParent.childCount > 0)
        {
            DestroyImmediate(spawnParent.GetChild(0).gameObject);
        }
        for (int x = 0; x < gridXSize; x++)
        {
            for (int z = 0; z < gridZSize; z++)
            {
                lettersRowColumn[x, z] = null;
            }
        }
        for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
        {
            words[wordIndex].SpawnedList.Clear();
        }
        totalSpawnedList.Clear();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WordSearch))]
    public class WordSearch_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // for other non-HideInInspector fields

            WordSearch script = (WordSearch)target;

            if (GUILayout.Button("Generate letters in scene"))
            {
                script.GenerateLetters();
            }
            if (GUILayout.Button("Reset"))
            {
                script.ResetAll();
            }
        }
    }
#endif
}
