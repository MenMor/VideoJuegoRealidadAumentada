using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static QtsData;

public class QuestionManager : MonoBehaviour
{
    public Text questionText;
    public Text scoreText;
    public Text FinalScore;
    public Button[] replyButtons;
    public GameObject Right;
    public GameObject Wrong;
    public GameObject GameFinished;

    public APIClient apiClient; // Referencia al script APIClient
    public int userId;

    private int currentQuestion = 0;
    private List<QuestionData> questionsFromDB = new List<QuestionData>();
    private static int score = 0;

    void Start()
    {
        userId = 17;
        apiClient.userId = userId;

        if (apiClient == null)
        {
            Debug.LogError("APIClient is not assigned.");
            return;
        }
        
        StartCoroutine(InitializeQuestions());
        Right.gameObject.SetActive(false);
        Wrong.gameObject.SetActive(false);
        GameFinished.gameObject.SetActive(false);
    }

    IEnumerator InitializeQuestions()
    {
        yield return StartCoroutine(apiClient.GetQuestions());
        if (apiClient.questionsList != null)
        {
            questionsFromDB = apiClient.questionsList;
            if (questionsFromDB.Count > 0)
            {
                SetQuestion(currentQuestion);
            }
            else
            {
                Debug.LogError("No questions retrieved from API.");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve questions from APIClient.");
        }
    }

    void SetQuestion(int questionIndex)
    {
        if (questionsFromDB == null || questionsFromDB.Count == 0)
        {
            Debug.LogError("No questions available to set.");
            return;
        }

        QuestionData questionData = questionsFromDB[questionIndex];
        questionText.text = questionData.question;

        foreach (Button r in replyButtons)
        {
            r.onClick.RemoveAllListeners();
        }

        string[] replies = questionData.GetReplies();
        for (int i = 0; i < replyButtons.Length; i++)
        {
            replyButtons[i].GetComponentInChildren<Text>().text = replies[i];
            int replyIndex = i;
            replyButtons[i].onClick.AddListener(() =>
            {
                StartCoroutine(CheckReply(questionData.id, replyIndex));
            });
        }
    }

    IEnumerator CheckReply(int questionId, int replyIndex)
    {
        yield return StartCoroutine(apiClient.HasUserAnsweredQuestion(userId, questionId, (hasAnswered, isCorrectPreviously) =>
        {
            bool isCorrectNow = replyIndex == questionsFromDB[currentQuestion].correct_reply_index;

            if (!hasAnswered)
            {
                StartCoroutine(apiClient.MarkQuestionAsAnswered(userId, questionId, isCorrectNow));

                if (isCorrectNow)
                {
                    score++;
                    scoreText.text = "" + score;
                    Right.SetActive(true); 
                }
                else
                {
                    Wrong.SetActive(true); 
                }
            }
            else
            {
                if (isCorrectPreviously)
                {
                    Right.SetActive(true); 
                }
                else
                {
                    Wrong.SetActive(true); 
                }

                Debug.Log("Question already answered. No additional points awarded.");
            }

            StartCoroutine(Next());
        }));
    }



    IEnumerator Next()
    {
        yield return new WaitForSeconds(2);
        currentQuestion++;
        if (currentQuestion < questionsFromDB.Count)
        {
            Reset();
        }
        else
        {
            GameFinished.SetActive(true);
            float scorePercentage = (float)score / questionsFromDB.Count * 100;
            FinalScore.text = "Su puntuación es " + score.ToString("F0");
            if (scorePercentage < 50)
            {
                FinalScore.text += "\nGame Over";
            }
            else if (scorePercentage < 60)
            {
                FinalScore.text += "\nkeep Trying";
            }
            else if (scorePercentage < 70)
            {
                FinalScore.text += "\nGood Job";
            }
            else if (scorePercentage < 80)
            {
                FinalScore.text += "\nWell Done!";
            }
            else
            {
                FinalScore.text += "\nYou're a genius!";
            }
            apiClient.UpdateUserPoints(userId, score);
        }
    }

    private void Reset()
    {
        Right.SetActive(false);
        Wrong.SetActive(false);
        foreach (Button r in replyButtons)
        {
            r.interactable = true;
        }
        SetQuestion(currentQuestion);
    }

}