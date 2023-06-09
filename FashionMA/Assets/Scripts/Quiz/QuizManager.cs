using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// 퀴즈 UI 관리 스크립트
public class QuizManager : MonoBehaviour
{
    [SerializeField] Text text_title;
    [SerializeField] Text text_queistion;
    [SerializeField] Button btn_O;
    [SerializeField] Button btn_X;
    [SerializeField] Text text_O;
    [SerializeField] Text text_X;
    [SerializeField] Text text_check;

    private int QuizNum = 0;
    private int score = 0;
    private string date = "";

    private int[] rand = { 0, 0, 0, 0, 0 };

    List<Dictionary<string, object>> data;

    private void Awake()
    {
        data = CSVReader.Read("Questions");
        /*for (var i = 0; i < data.Count; i++)
        {
            Debug.Log("Num " + data[i]["Num"] + " " +
                   "Question " + data[i]["Question"] + " " +
                   "Answer " + data[i]["Answer"]);
        }*/

        rand = UniqueRandom(5, 0, 9);

        for(int i = 0; i < rand.Length; i++)
        {
            Debug.Log("rand[i] : " + rand[i]);
        }

        UniteData.LoadLeaderboard();
    }

    // Start is called before the first frame update
    void Start()
    {
        QuizNum = 0;
        score = 0;
        date = "";

        text_check.text = "";
        text_title.text = "퀴즈풀기";
        text_queistion.text = "날씨 관련 문제를 풀어볼래?";
        text_O.text = "퀴즈시작!";
        text_X.text = "점수판으로!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartQuiz()
    {
        QuizNum = 0;
        score = 0;

        NextQuestion(); 
    }

    public void SelectO()
    {
        if (QuizNum == 0 || QuizNum == 6)
        {
            StartQuiz();
        }
        else CheckAnswer(0);
    }

    public void SelectX()
    {
        if (QuizNum == 0 || QuizNum == 6)
        {
            SceneManager.LoadScene("QuizScore");
        }
        else CheckAnswer(1);
    }

    private void CheckAnswer(int answer)
    {
        btn_O.enabled = false;
        btn_X.enabled = false;

        // 정답이면
        if (answer == int.Parse(data[rand[QuizNum - 1]]["Answer"].ToString()))
        {
            text_check.text = "O";
            StartCoroutine(ShowCheckAnswer());
            score = score + 20;
        }

        // 오답이면

        else
        {
            text_check.text = "X";
            StartCoroutine(ShowCheckAnswer());
        }
         
    }

    private void NextQuestion()
    {
        QuizNum++;

        // 모든 문제 끝
        if (QuizNum == 6)
        {
            text_title.text = "";
            text_queistion.text = "축하해!\n" + score.ToString() + "점을 받았어!";
            text_O.text = "다시 할래!";
            text_X.text = "점수판으로!";

            // 현재 시간 및 점수 기록
            date = DateTime.Now.ToString(("yyyy.MM.dd HH:mm"));
            AddEntry(score, date);
        }

        else
        {
            // 문제 & 답 세팅

            text_queistion.text = data[rand[QuizNum-1]]["Question"].ToString();
            text_O.text = "맞아!";
            text_X.text = "아니야!";

            text_title.text = QuizNum.ToString() + "번문제";
        }
    }

    private IEnumerator ShowCheckAnswer()
    {
        text_check.color = new Color(text_check.color.r, text_check.color.g, text_check.color.b, 1);
        while (text_check.color.a > 0.0f)
        {
            text_check.color = new Color(text_check.color.r, text_check.color.g, text_check.color.b, text_check.color.a - (Time.deltaTime / 0.8f));
            yield return null;
        }

        btn_O.enabled = true;
        btn_X.enabled = true;

        NextQuestion();
    }

    // 중복 없이 난수 생성
    private int[] UniqueRandom(int count, int min, int max)
    {

        int[] rand = new int[count];
        int[] range = new int[max - min + 1];

        // 배열 초기화
        for (int i = 0; i < range.Length; i++)
        {
            range[i] = min + i;
        }

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, range.Length);
            rand[i] = range[randomIndex];

            // 뽑은 숫자와 마지막 숫자 교환
            range[randomIndex] = range[i];
            range[i] = rand[i];
        }

        return rand;
    }

    private void AddEntry(int score, string date)
    {
        // 배열이 꽉 찬 경우 기존 값보다 점수가 클 때만 추가
        if (UniteData.logScore.Length >= UniteData.maxEntries)
        {
            if (UniteData.logScore[UniteData.maxEntries - 1] < score)
            {
                // 새로운 값 추가
                UniteData.logScore[UniteData.maxEntries - 1] = score;
                UniteData.logDate[UniteData.maxEntries - 1] = date;

                // 값 정렬
                SortLeaderboard();

                UniteData.SaveUserData();
            }
        }

        else
        {
            // 새로운 값 추가
            UniteData.logScore[UniteData.maxEntries - 1] = score;
            UniteData.logDate[UniteData.maxEntries - 1] = date;

            // 값 정렬
            SortLeaderboard();

            UniteData.SaveUserData();
        }
    }
    private void SortLeaderboard()
    {
        for (int i = UniteData.maxEntries - 1; i > 0; i--)
        {
            // 새로운 값과 비교
            if (UniteData.logScore[i] > UniteData.logScore[i - 1])
            {
                // 점수가 더 큰 경우 값 교환
                SwapValues(i, i - 1);
            }
            else if (UniteData.logScore[i] == UniteData.logScore[i - 1])
            {
                // 점수가 같은 경우 날짜 비교
                DateTime currentDate = DateTime.Parse(UniteData.logDate[i]);
                DateTime previousDate = DateTime.Parse(UniteData.logDate[i - 1]);

                if (currentDate < previousDate)
                {
                    // 날짜가 더 작은 경우 값 교환
                    SwapValues(i, i - 1);
                }
            }
        }
    }

    private void SwapValues(int index1, int index2)
    {
        int tempScore = UniteData.logScore[index1];
        UniteData.logScore[index1] = UniteData.logScore[index2];
        UniteData.logScore[index2] = tempScore;

        string tempDate = UniteData.logDate[index1];
        UniteData.logDate[index1] = UniteData.logDate[index2];
        UniteData.logDate[index2] = tempDate;
    }
}

