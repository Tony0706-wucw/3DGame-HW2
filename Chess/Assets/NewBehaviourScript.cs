//参考:https://blog.csdn.net/zzlyw/article/details/54345250?ops_request_misc=%257B%2522request%255Fid%2522%253A%2522166268601716782417072829%2522%252C%2522scm%2522%253A%252220140713.130102334..%2522%257D&request_id=166268601716782417072829&biz_id=0&utm_medium=distribute.pc_search_result.none-task-blog-2~all~top_positive~default-1-54345250-null-null.142^v47^control,201^v3^control&utm_term=unity%E4%BA%94%E5%AD%90%E6%A3%8B&spm=1018.2226.3001.4187
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    //四个锚点位置，用于计算棋子落点
    public GameObject LeftTop;
    public GameObject RightTop;
    public GameObject LeftBottom;
    public GameObject RightBottom;
    //主摄像机
    public Camera cam;
    //锚点在屏幕上的映射位置
    Vector3 LTPos;
    Vector3 RTPos;
    Vector3 LBPos;
    Vector3 RBPos;

    Vector3 PointPos;//当前点选的位置
    float gridWidth = 1; //棋盘网格宽度
    float gridHeight = 1; //棋盘网格高度
    float minGridDis;  //网格宽和高中较小的一个
    Vector2[,] chessPos; //存储棋盘上所有可以落子的位置
    int[,] chessState; //存储棋盘位置上的落子状态
    enum turn { black, white };
    turn chessTurn; //落子顺序
    public Texture2D white; //白棋子
    public Texture2D black; //黑棋子
    public Texture2D blackWin; //白子获胜提示图
    public Texture2D whiteWin; //黑子获胜提示图
    int winner = 0; //获胜方，1为黑子，-1为白子
    bool isPlaying = true; //是否处于对弈状态

    void Start()
    {
        chessPos = new Vector2[15, 15];
        chessState = new int[17, 16];/*原来定义是new int[15, 15]，这里将原来数组chessState上、下和右边各加一排数据，
        也就相当于在棋盘的上、下和右边各填加一排隐形的棋道。原因后面解释*/
        chessTurn = turn.black;

        //计算锚点位置
        LTPos = cam.WorldToScreenPoint(LeftTop.transform.position);
        RTPos = cam.WorldToScreenPoint(RightTop.transform.position);
        LBPos = cam.WorldToScreenPoint(LeftBottom.transform.position);
        RBPos = cam.WorldToScreenPoint(RightBottom.transform.position);

        //计算网格宽度
        gridWidth = (RTPos.x - LTPos.x) / 14;
        gridHeight = (LTPos.y - LBPos.y) / 14;
        minGridDis = gridWidth < gridHeight ? gridWidth : gridHeight;

        //计算落子点位置
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                chessPos[i, j] = new Vector2(LBPos.x + gridWidth * j, LBPos.y + gridHeight * i);//这里和源程序定义稍有不同，这里i定位行，j为列
            }
        }
    }

    void Update()
    {
        //检测鼠标输入并确定落子状态
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            PointPos = Input.mousePosition;
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    //找到最接近鼠标点击位置的落子点，如果空则落子
                    if (Dis(PointPos, chessPos[i, j]) < minGridDis / 2 && chessState[i + 1, j] == 0)/*这里chessState行要加1，
                        因为上、下和右边各多加了一排，要空出来，chessPos的i行对应chessState的i+1行*/
                    {
                        //根据下棋顺序确定落子颜色
                        chessState[i + 1, j] = chessTurn == turn.black ? 1 : -1;//同理
                        //落子成功，更换下棋顺序
                        chessTurn = chessTurn == turn.black ? turn.white : turn.black;
                    }
                }
            }
            //调用判断函数，确定是否有获胜方
            int re = result();
            if (re == 1)
            {
                Debug.Log("黑棋胜");
                winner = 1;
                isPlaying = false;
            }
            else if (re == -1)
            {
                Debug.Log("白棋胜");
                winner = -1;
                isPlaying = false;
            }
        }
        //按下空格重新开始游戏
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    chessState[i + 1, j] = 0;//同理
                }
            }
            isPlaying = true;
            chessTurn = turn.black;
            winner = 0;
        }
    }
    //计算平面距离函数
    float Dis(Vector3 mPos, Vector2 gridPos)
    {
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }

    void OnGUI()
    {
        //绘制棋子
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (chessState[i + 1, j] == 1)//同理
                {
                    GUI.DrawTexture(new Rect(chessPos[i, j].x - gridWidth / 2, Screen.height - chessPos[i, j].y - gridHeight / 2, gridWidth, gridHeight), black);
                }
                if (chessState[i + 1, j] == -1)//同理
                {
                    GUI.DrawTexture(new Rect(chessPos[i, j].x - gridWidth / 2, Screen.height - chessPos[i, j].y - gridHeight / 2, gridWidth, gridHeight), white);
                }
            }
        }
        //根据获胜状态，弹出相应的胜利图片
        if (winner == 1)
        {
            GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), blackWin);
        }
        if (winner == -1)
            GUI.DrawTexture(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.25f), whiteWin);
    }
	//改写result函数
 	/*解释：C语言中，这样的表达式：chessState[i]&&chessState[i+1]&&chessState[i+2]&&chessState[i+3]&&chessState[i+4]，如果
     * chessState[i]为False,则不管B是真是假或者是异常都不会运行，利用这一点，在chessState的右边、上边和下边各加一行为0的数据，
     * 这样在判断连续五个棋子的状态时，就不用担心chessState数组的索引值超出范围。例如：chessState[i+4]的索引值i+4刚好超出范围，
     * 通过在原来数组chessState的上、下和右边个添加一排为0的数，这样chessState[i+3]==0，于是就可以避免引起异常，从而简化代码*/
    int result()
    {
        int flag = 0;
        if (chessTurn == turn.white)
        {
            for (int i = 1; i <= 15; i++)//这里的i从1开始
            {
                for (int j = 0; j <= 14; j++)//j不用变
                {
                    if ((chessState[i, j] == 1 && chessState[i, j + 1] == 1 && chessState[i, j + 2] == 1 && chessState[i, j + 3] == 1 && chessState[i, j + 4] == 1)//向右横向
                        || (chessState[i, j] == 1 && chessState[i + 1, j] == 1 && chessState[i + 2, j] == 1 && chessState[i + 3, j] == 1 && chessState[i + 4, j] == 1)//向上横向
                        || (chessState[i, j] == 1 && chessState[i + 1, j + 1] == 1 && chessState[i + 2, j + 2] == 1 && chessState[i + 3, j + 3] == 1 && chessState[i + 4, j + 4] == 1)//向右上斜向
                        || (chessState[i, j] == 1 && chessState[i - 1, j + 1] == 1 && chessState[i - 2, j + 2] == 1 && chessState[i - 3, j + 3] == 1 && chessState[i - 4, j + 4] == 1))//向右下斜向
                    {
                        flag = 1;
                    }
                }
            }
        }
        else if (chessTurn == turn.black)
        {
            for (int i = 1; i <= 15; i++)//这里的i从1开始
            {
                for (int j = 0; j <= 14; j++)
                {

                    if ((chessState[i, j] == -1 && chessState[i, j + 1] == -1 && chessState[i, j + 2] == -1 && chessState[i, j + 3] == -1 && chessState[i, j + 4] == -1)
                        || (chessState[i, j] == -1 && chessState[i + 1, j] == -1 && chessState[i + 2, j] == -1 && chessState[i + 3, j] == -1 && chessState[i + 4, j] == -1)
                        || (chessState[i, j] == -1 && chessState[i + 1, j + 1] == -1 && chessState[i + 2, j + 2] == -1 && chessState[i + 3, j + 3] == -1 && chessState[i + 4, j + 4] == -1)
                        || (chessState[i, j] == -1 && chessState[i - 1, j + 1] == -1 && chessState[i - 2, j + 2] == -1 && chessState[i - 3, j + 3] == -1 && chessState[i - 4, j + 4] == -1))
                    {
                        flag = -1;
                    }
                }
            }
        }
        return flag;
    }
}
