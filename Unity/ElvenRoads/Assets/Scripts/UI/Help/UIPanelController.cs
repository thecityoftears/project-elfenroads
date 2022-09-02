using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelController : MonoBehaviour
{


    [SerializeField] private GameObject m_panel;
    [SerializeField] private Text m_bookTitle;
    [SerializeField] private List<GameObject> m_bookObjects;
    
    private Dictionary<string, Book> m_books;
    
    private Book m_curBook;

    private void Awake()
    {

        m_books = new Dictionary<string, Book>();
        foreach (var book in m_bookObjects.ConvertAll(book => book.GetComponent<Book>()))
            m_books.Add(book.gameObject.name, book);

        DontDestroyOnLoad(gameObject);
    }

    #region UI Callback Functions
    public void OnOpenPanel()
    {
        m_panel.SetActive(true);

        OnChangeBook("ElfenLandBook");    
    }

    public void OnClosePanel()
    {
        m_panel.SetActive(false);
    }

    public void OnJumpToPage(int pPageIndex)
    {
        if(m_curBook)
           m_curBook.JumpToPage(pPageIndex - 1);
    }

    public void OnChangeBook(string pBookName)
    {
        if (m_curBook)
            m_curBook.gameObject.SetActive(false);

        m_curBook = m_books[pBookName];
        m_curBook.gameObject.SetActive(true);
        m_bookTitle.text = pBookName.Substring(0, 9);
    }

    #endregion
}
