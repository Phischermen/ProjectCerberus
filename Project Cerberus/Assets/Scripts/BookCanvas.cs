using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BookCanvas : MonoBehaviour
{
    // Initialized via inspector
    // TODO: Sprites for next and previous are missing.
    [HideInInspector] public Button next;
    [HideInInspector] public Button prev;
    [HideInInspector] public Image nextImage;
    [HideInInspector] public Sprite nextSprite;
    [HideInInspector] public Sprite playSprite;
    [HideInInspector] public Sprite closeSprite;

    [HideInInspector] public Image image;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public AudioClip pageTurn;
    [HideInInspector] public AudioClip bookOpen;
    [HideInInspector] public AudioClip bookClose;

    public Sprite[] pages;
    public Action OnNextPressedOnLastPage;
    public Action<int> OnPageOpen;
    private int _page;

    public int page
    {
        get { return _page; }
        set
        {
            value = (int) Mathf.Clamp(value, 0f, pages.Length);
            _page = value;
            image.sprite = pages[value];

            if (page == pages.Length - 1)
            {
                nextImage.sprite = cutsceneVersion ? playSprite : closeSprite;
            }
            else
            {
                nextImage.sprite = nextSprite;
            }

            if (page == 0)
            {
                prev.gameObject.SetActive(false);
            }
            else
            {
                prev.gameObject.SetActive(true);
            }
        }
    }

    public bool onLastPage => page == pages.Length - 1;

    public bool open;

    public bool cutsceneVersion = true;

    // Start is called before the first frame update
    void Start()
    {
        page = 0;
        next.onClick.AddListener(NextPressed);
        prev.onClick.AddListener(PrevPressed);
    }

    void NextPressed()
    {
        if (!onLastPage)
        {
            audioSource.PlayOneShot(pageTurn);
            page += 1;
            OnPageOpen?.Invoke(page);
            prev.gameObject.SetActive(true);
        }
        else
        {
            OnNextPressedOnLastPage?.Invoke();
        }
    }

    void PrevPressed()
    {
        if (page != 0)
        {
            audioSource.PlayOneShot(pageTurn);
            page -= 1;
            OnPageOpen?.Invoke(page);
        }
    }

    public void OpenManual()
    {
        if (open) return;
        open = true;
        audioSource.PlayOneShot(bookOpen);
        GetComponent<Canvas>().enabled = true;
    }

    public void CloseManual()
    {
        if (!open) return;
        open = false;
        audioSource.PlayOneShot(bookClose);
        GetComponent<Canvas>().enabled = false;
    }
}