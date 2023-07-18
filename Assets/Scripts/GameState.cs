using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Topebox.Tankwars
{
    public class GameState : MonoBehaviour
    {
        public Vector2 Player1Position;
        public Vector2 Player2Position;

        public GameConfig Config;
        private Constants.CellType[,] logicMap;
        private Cell[,] displayMap;
        public Cell cellPrefab;

        public Tank tankPrefab;
        public Tank player1Tank;
        public Tank player2Tank;
        public Transform TankParent;

        public int CurrentPlayer = 1;
        public int CurrentTurn = 0;
        public List<Vector2> Player1Moves = new List<Vector2>();
        public List<Vector2> Player2Moves = new List<Vector2>();

        public bool IsGameOver = false;
        [FormerlySerializedAs("IsMoving")] public bool CanMove = false;

        public void UpdateMove()
        {
            if (!CanMove || IsGameOver)
            {
                Debug.LogError("IsMoving:" + CanMove + " IsGameOver:" + IsGameOver);
                return;
            }
            
            
            var winPlayer = CheckGameOver();
            if (winPlayer != Constants.GameResult.PLAYING)
            {
                IsGameOver = true;
                Debug.LogError($" IsGameOver {IsGameOver} Result {winPlayer}");
                switch (winPlayer)
                {
                    case Constants.GameResult.PLAYER1_WIN:
                        TextScore.text = "Game Over Player 1 Win";
                        break;
                    case Constants.GameResult.PLAYER2_WIN:
                        TextScore.text = "Game Over Player 2 Win";
                        break;
                    case Constants.GameResult.DRAW:
                        TextScore.text = "Game Over Draw";
                        break;
                }
                
                //Export Orginal Map and Moves of players and winner
                return;
            }

            if (player1Tank.PlayerId == CurrentPlayer)
            {
                var hasMoveP1 = HasValidMove(player1Tank.CurrentCell);
                if (hasMoveP1)
                {
                    var move = UpdateMoveForTank(player1Tank, player2Tank);
                    Player1Moves.Add(move);
                }
                else
                {
                    Debug.Log("No move left for player1 => player2 turn");
                }

                CurrentPlayer = player2Tank.PlayerId;
            }
            else if (player2Tank.PlayerId == CurrentPlayer)
            {
                var hasMoveP2 = HasValidMove(player2Tank.CurrentCell);
                if (hasMoveP2)
                {
                    var move = UpdateMoveForTank(player2Tank, player1Tank);
                    Player2Moves.Add(move);
                }
                else
                {
                    Debug.Log("No move left for player2 => player1 turn");
                }

                CurrentPlayer = player1Tank.PlayerId;
            }

        }

        private Constants.GameResult CheckGameOver()
        {
            var hasMoveP1 = HasValidMove(player1Tank.CurrentCell);
            var hasMoveP2 = HasValidMove(player2Tank.CurrentCell);
            if (!hasMoveP1 && hasMoveP2)
            {
                return Constants.GameResult.PLAYER2_WIN;
            }

            if (hasMoveP1 && !hasMoveP2)
            {
                return Constants.GameResult.PLAYER1_WIN;
            }

            if (!hasMoveP1 && !hasMoveP2)
            {
                if (ScoreRed > ScoreBlue)
                    return Constants.GameResult.PLAYER1_WIN;
                if (ScoreRed < ScoreBlue)
                    return Constants.GameResult.PLAYER2_WIN;
                return Constants.GameResult.DRAW;
            }

            return Constants.GameResult.PLAYING; //not over
        }

        private bool HasValidMove(Vector2 currentCell)
        {
            var upCell = GetNextCell(currentCell, Constants.Direction.UP);
            if (IsValidCell(upCell))
            {
                return true;
            }

            var downCell = GetNextCell(currentCell, Constants.Direction.DOWN);
            if (IsValidCell(downCell))
            {
                return true;
            }

            var leftCell = GetNextCell(currentCell, Constants.Direction.LEFT);
            if (IsValidCell(leftCell))
            {
                return true;
            }

            var rightCell = GetNextCell(currentCell, Constants.Direction.RIGHT);
            if (IsValidCell(rightCell))
            {
                return true;
            }

            return false;
        }

        public Vector2 UpdateMoveForTank(Tank currentTank, Tank otherTank)
        {
            var direction = currentTank.GetNextMove(this, logicMap, otherTank.CurrentCell);
            var nextCell = GetNextCell(currentTank.CurrentCell, direction);
            //CheckValidDirection
            if (IsValidCell(nextCell))
            {
                currentTank.SetCurrentCell(nextCell);

                var position = GetPosition(nextCell);
                CanMove = false;
                currentTank.transform.DORotate(GetRotateByDirection(direction), 0.5f);
                currentTank.transform.DOMove(new Vector3(position.x, position.y, 0), 1f).OnComplete(() =>
                {
                    CanMove = true;
                    OccupyPosition(nextCell, currentTank.CurrentTank);
                });
                return nextCell;
            }
            else
            {
                Debug.LogError(
                    $"Your Direction Is Invalid Direction:{direction} CurrentCell:{currentTank.CurrentCell}");
            }

            return new Vector2(-1, -1);
        }

        private Vector3 GetRotateByDirection(Constants.Direction direction)
        {
            switch (direction)
            {
                case Constants.Direction.UP:
                    return new Vector3(0, 0, 180);
                case Constants.Direction.LEFT:
                    return new Vector3(0, 0, 270);
                case Constants.Direction.DOWN:
                    return new Vector3(0, 0, 0);
                case Constants.Direction.RIGHT:
                    return new Vector3(0, 0, 90);
            }

            return Vector3.zero;
        }

        public bool IsValidCell(Vector2 nextCell)
        {
            if (nextCell.x < 0 || nextCell.x >= Config.MapWidth)
                return false;
            if (nextCell.y < 0 || nextCell.y >= Config.MapHeight)
                return false;

            return logicMap[(int)nextCell.x, (int)nextCell.y] == Constants.CellType.EMPTY;
        }

        private void OccupyPosition(Vector2 cell, Constants.TankType tankType)
        {
            if (logicMap[(int)cell.x, (int)cell.y] == Constants.CellType.EMPTY)
            {
                switch (tankType)
                {
                    case Constants.TankType.RED:
                        logicMap[(int)cell.x, (int)cell.y] = Constants.CellType.RED;
                        IncreaseScore(1, Constants.TankType.RED);
                        break;
                    case Constants.TankType.BLUE:
                        logicMap[(int)cell.x, (int)cell.y] = Constants.CellType.BLUE;
                        IncreaseScore(1, Constants.TankType.BLUE);
                        break;
                }

                UpdateMap();
            }
        }

        public int ScoreRed = 0;
        public int ScoreBlue = 0;

        private void IncreaseScore(int score, Constants.TankType tankType)
        {
            switch (tankType)
            {
                case Constants.TankType.RED:
                    ScoreRed += score;
                    break;
                case Constants.TankType.BLUE:
                    ScoreBlue += score;
                    break;
            }

            UpdateScoreUI();
        }


        public Vector2 GetNextCell(Vector2 currentPosition, Constants.Direction direction)
        {
            switch (direction)
            {
                case Constants.Direction.UP:
                    return new Vector2(currentPosition.x, currentPosition.y - 1);
                case Constants.Direction.LEFT:
                    return new Vector2(currentPosition.x - 1, currentPosition.y);
                case Constants.Direction.DOWN:
                    return new Vector2(currentPosition.x, currentPosition.y + 1);
                case Constants.Direction.RIGHT:
                    return new Vector2(currentPosition.x + 1, currentPosition.y);
            }

            return currentPosition;
        }

        void Start()
        {
            logicMap = new Constants.CellType[Config.MapWidth, Config.MapHeight];
            displayMap = new Cell[Config.MapWidth, Config.MapHeight];
            GenerateMap();
            UpdateMap();
            player1Tank = CreateTank(Config.Player1Type, 1);
            player1Tank.transform.DORotate(GetRotateByDirection(Constants.Direction.DOWN), 0.1f);

            player2Tank = CreateTank(Config.Player2Type, 2);
            player2Tank.transform.DORotate(GetRotateByDirection(Constants.Direction.UP), 0.1f);
            CanMove = false;

            StartGame();
        }

        void StartGame()
        {
            ScoreRed = 0;
            ScoreBlue = 0;
            CurrentPlayer = 1;
            CurrentTurn = 0;
            Player1Moves.Clear();
            Player2Moves.Clear();


            player1Tank.SetCurrentCell(Player1Position);
            var position = GetPosition(Player1Position);
            player1Tank.transform.position = position;
            OccupyPosition(Player1Position, player1Tank.CurrentTank);

            player2Tank.SetCurrentCell(Player2Position);
            position = GetPosition(Player2Position);
            player2Tank.transform.position = position;
            OccupyPosition(Player2Position, player2Tank.CurrentTank);

            CanMove = true;
            IsGameOver = false;
        }

        public TextMeshProUGUI TextScore;

        private void UpdateScoreUI()
        {
            TextScore.text = $"<color=red>{ScoreRed}</color> - <color=blue>{ScoreBlue}</color>";
        }

        private Tank CreateTank(Constants.TankType tankType, int playerId)
        {
            var tank = Instantiate<Tank>(tankPrefab, new Vector3(0, 0, 0), Quaternion.identity,
                TankParent);
            tank.SetType(tankType);
            tank.SetId(playerId);
            return tank;
        }

        public Vector2 GetPosition(Vector2 cellPos)
        {
            return GetPosition(cellPos.x, cellPos.y);
        }

        public Vector2 GetPosition(float cellX, float cellY)
        {
            return new Vector2(cellX, -cellY);
        }

        private void UpdateMap()
        {
            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    displayMap[x, y].SetType(logicMap[x, y]);
                }
            }
        }

        private void GenerateMap()
        {
            //generate random symmetric wall
            for (int i = 0; i < Config.WallCount / 2; i++)
            {
                var x = 0;
                var y = 0;
                while (x == 0 && y == 0)
                {
                    x = Random.Range(0, Config.MapWidth / 2);
                    y = Random.Range(0, Config.MapHeight / 2);
                }

                logicMap[x, y] = Constants.CellType.WALL;
                logicMap[Config.MapWidth - x - 1, Config.MapHeight - y - 1] = Constants.CellType.WALL;
            }

            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    if (displayMap[x, y] == null)
                    {
                        var pos = GetPosition(new Vector2(x, y));
                        var cell = Instantiate(cellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity,
                            transform);
                        displayMap[x, y] = cell;
                    }
                }
            }
        }
    }
}