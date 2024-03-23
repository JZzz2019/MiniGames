using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using DG.Tweening;
using Bridge.Scripts;

//[RequireComponent(typeof(InteractableUnityEventWrapper))]
//[RequireComponent(typeof(OutlineController))]
public class Letter : MonoBehaviour
{
    public enum Alphabet
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z
    }

    public Alphabet ThisAlphabet = Alphabet.A;

    //private OutlineController outlineController;
    //public OutlineController OutlineController => outlineController;

    //This has to be serialised to assign the swapped card
    [SerializeField] private Letter swappedCard = null;

    public Letter SwappedCard
    {
        get { return swappedCard; }
        set { swappedCard = value; }
    }

    public bool IsLocked
    {
        get { return isLocked; }
        set { isLocked = value; }
    }

    private bool isLocked = false;

    private bool isDone = false;
    public bool IsDone
    {
        get { return isDone; }
        set { isDone = value; }
    }

    private bool isEnabled = true;
    public bool IsEnabled
    {
        get { return isEnabled; }
        set { isEnabled = value; }
    }

    private bool isShownAsOption = false;
    public bool IsShownAsOption
    {
        get { return isShownAsOption; }
        set { isShownAsOption = value; }
    }

    private bool isSelected = false;
    public bool IsSelected => isSelected;

    [SerializeField] private int rowAxis;
    [SerializeField] private int columnAxis;

    public int RowAxis
    {
        get { return rowAxis; }
        set { rowAxis = value; }
    }

    public int ColumnAxis
    {
        get { return columnAxis; }
        set { columnAxis = value; }
    }

    private Action OnHover;
    private Action UnHover;
    private Action OnSelect;

    public Action<Letter> OnSelectedLetter;
    public Action OnDeselect;

    private void Awake()
    {
        OnHover += EnableOutline;
        OnSelect += Select;
    }
    private void Start()
    {
        //outlineController = GetComponent<OutlineController>();
        //outlineController.DisableColouredOutline();

        //transform.GetComponent<InteractableUnityEventWrapper>().WhenHover.AddListener(() => OnHover?.Invoke());
        //transform.GetComponent<InteractableUnityEventWrapper>().WhenUnhover.AddListener(() => UnHover?.Invoke());
        //transform.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => OnSelect?.Invoke());
    }

    private void EnableOutline()
    {
        if (isDone) { return; }

        if (!isEnabled) { return; }

        if (isSelected) { return; }

        OnHover -= EnableOutline;
        //outlineController.ChangeColour(Color.white);
        //outlineController.EnableColouredOutline();
        UnHover += DisableOutline;
    }
    private void DisableOutline()
    {
        if (isDone) { return; }

        if (!isEnabled) { return; }

        if (isSelected) { return; }

        UnHover -= DisableOutline;
        if (isShownAsOption)
        {
            //outlineController.ChangeColour(Color.grey);
        }
        else
        {
            //outlineController.ChangeColour(Color.white);
            //outlineController.DisableColouredOutline();
        }
        OnHover += EnableOutline;
    }

    private void Select()
    {
        if (isDone) { return; }

        if (!isEnabled) { return; }

        //Check if this already selected open card has been selected again
        if (isSelected)
        {
            isSelected = false;
            DisableOutline();
            OnDeselect?.Invoke();
            return;
        }
        EnableOutline();
        isSelected = true;
        OnSelectedLetter?.Invoke(this);
    }

    public void ResetLetter()
    {
        isSelected = false;
        DisableOutline();
    }

    public void Swap()
    {
        if (swappedCard == null) { return; }

        Vector3 destinationPosition = swappedCard.transform.localPosition;

        //transform.DOLocalMove(destinationPosition, 0.4f)
        //    .SetEase(Ease.InOutSine);
    }

    public void IncorrectSwap()
    {
        //outlineController.ChangeColour(Color.red);
    }
}
