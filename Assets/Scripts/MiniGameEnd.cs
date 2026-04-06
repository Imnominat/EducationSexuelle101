using UnityEngine;
using System.Collections;
using TMPro;

public enum QuestionType
{
    TrueFalse,
    MultipleChoice
}

[System.Serializable]
public class QuestionData
{
    public string id;
    public string question;
    public string choicesA;
    public string choicesB;
    public string choicesC;
    public string choicesD;
    public string explanation;

}

[System.Serializable]
public class QuestionDatabase
{
    public QuestionData[] questions;
}

public class MiniGameEnd : MonoBehaviour
{
    public static int score = 0;

    [Header("Settings")]
    public QuestionType questionType = QuestionType.TrueFalse;
    private QuestionDatabase questionDB;
    public string questionID;

    [Header("UI")]
    public TextMeshPro messageQuestionTMP;
    public TextMeshProUGUI scoreTMP;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failureSound;
    public AudioClip voiceClip;

    [Header("Interactables")]
    public GameObject Buzzers;
    public GameObject Question;
    public GameObject Next;
    public GameObject Pupitre;

    public GameObject door;

    public float doorDelay = 3f;

    [Header("Buzzer Movement")]
    public float pokeDistance = 0.02f;
    public float pokeSpeed = 5f;
    private Vector3 initialPos;
    private bool isPoked = false;
    private static bool isShowingExplanation = false;

    

    void Start()
    {
        initialPos = transform.localPosition;
        LoadExplanations();
        if (messageQuestionTMP != null && !isShowingExplanation)
        {
            messageQuestionTMP.text = GetQuestion(questionID);
            messageQuestionTMP.text += GetAllChoices(questionID);
        }
    }

    void Update()
    {
        Vector3 targetPos = isPoked ? initialPos - new Vector3(0, pokeDistance, 0) : initialPos;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * pokeSpeed);
    }

    public void OnPokeEnter()
    {
        isPoked = true;
    }

    public void OnPokeExit()
    {
        isPoked = false;
    }

    public void OnAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            score += 1;
        }
        else
        {
            score -= 1;
        }

        if (scoreTMP != null)
        {
            scoreTMP.text = "Score : " + score;
        }

        

        if (audioSource != null)
        {
            audioSource.clip = isCorrect ? successSound : failureSound;
            audioSource.Play();
        }



        if (Buzzers != null)
        {
            if (isCorrect){
                Buzzers.SetActive(false);
                
                if (Next != null)
                {
                    Next.SetActive(true);
                    isShowingExplanation = true;
                    if (messageQuestionTMP != null)
                    {
                        messageQuestionTMP.text = GetExplanation(questionID);
                    }
                }
            }
        }
    }

     
     public void OnNext(bool pressed)
     {
        if (pressed)
        {
            if (Next != null)
            {
                Next.SetActive(false);
            }
            if (Question != null)
            {
                Question.SetActive(false);
            }
        }
     }
    



    void LoadExplanations()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("questions");
        if (jsonFile != null)
        {
            questionDB = JsonUtility.FromJson<QuestionDatabase>(jsonFile.text);
        }
    }

    string GetQuestion(string id)
    {
        if (questionDB == null || questionDB.questions == null)
            return "No question found.";

        foreach (var q in questionDB.questions)
        {
            if (q.id == id)
                return q.question;
        }
        return "No question found.";
    }
    string GetAllChoices(string id)
    {
        if (questionDB == null || questionDB.questions == null)
            return "No choices found.";

        foreach (var q in questionDB.questions)
        {
            if (q.id == id)
            {
                string choices = "\n \n";
                if (!string.IsNullOrEmpty(q.choicesA)) choices += q.choicesA + "\n";
                if (!string.IsNullOrEmpty(q.choicesB)) choices += q.choicesB + "\n";
                if (!string.IsNullOrEmpty(q.choicesC)) choices += q.choicesC + "\n";
                if (!string.IsNullOrEmpty(q.choicesD)) choices += q.choicesD + "\n";
                return choices;
            }
        }
        return "No choices found.";
    }
    string GetExplanation(string id)
    {
        if (questionDB == null || questionDB.questions == null)
            return "No explanation found.";

        foreach (var q in questionDB.questions)
        {
            if (q.id == id)
                return q.explanation;
        }
        return "No explanation found.";
    }
    
    public void ShowFinalScore()
    {
        if (scoreTMP != null)
        {
            scoreTMP.text = "Score final : " + score;
            scoreTMP.enabled = true;
        }
    }
}
