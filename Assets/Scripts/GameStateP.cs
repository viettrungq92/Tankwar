using System;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Mirror;
using Mirror.Examples.Pong;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Topebox.Tankwars
{
    public class GameStateP : NetworkBehaviour
    {
        public Vector2 Player1Position;
        public Vector2 Player2Position;

        public GameConfig Config;
        Constants.CellType[,] logicMap; 
        public Cell[,] displayMap;
        public Cell cellPrefab;

        public TankP tankPrefab;
        public TankP player1Tank;
        public TankP player2Tank;
        public Transform TankParent;
        
        [HideInInspector] public bool isOurTurn = false;
        public List<Vector2> Player1Moves = new List<Vector2>();
        public List<Vector2> Player2Moves = new List<Vector2>();
        
        public bool IsGameOver = false;
        public bool playerDone = false;
        public SyncListPlayerInfo players = new SyncListPlayerInfo();
        [FormerlySerializedAs("IsMoving")] public bool CanMove = false;

        public TextMeshProUGUI nameP1;
        public TextMeshProUGUI nameP2;
        public TextMeshProUGUI pointP1;
        public TextMeshProUGUI pointP2;

        public GameObject yourTurn;
        public int enemyId;
        
        
        //public GameObject enemyText;

        public void UpdateMove()
        {
            if (!CanMove || IsGameOver)
            {
                //Debug.LogError("IsMoving:" + CanMove + " IsGameOver:" + IsGameOver);
                return;
            }

            if (player1Tank.PlayerId == Player.localPlayer.PlayerId)
            {
                //var hasMoveP1 = HasValidMove(player1Tank.CurrentCell);
                //if (hasMoveP1)
                {
                    var direction = Constants.Direction.NO;
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        direction = Constants.Direction.UP;
                    }
                    else if (Input.GetKeyDown(KeyCode.S))
                    {
                        direction = Constants.Direction.DOWN;
                    }
                    else if (Input.GetKeyDown(KeyCode.A))
                    {
                        direction = Constants.Direction.LEFT;
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        direction = Constants.Direction.RIGHT;
                    }

                    if (direction != Constants.Direction.NO)
                    {
                        var nextCell = GetNextCell(player1Tank.CurrentCell, direction);
                        //CheckValidDirection
                        if (IsValidCell(nextCell))
                        {
                            var move = UpdateMoveForPlayer(player1Tank, direction);
                            Player1Moves.Add(move);
                            player1Tank.lastDir = direction;
                            playerDone = true;
                        }
                    }
                }

                if (playerDone)
                {
                    playerDone = false;
                    CmdSetTurn();
                }
            }
            else if (player2Tank.PlayerId == Player.localPlayer.PlayerId)
            {
                //var hasMoveP2 = HasValidMove(player2Tank.CurrentCell);
                //if (hasMoveP2)
                {
                    var direction = Constants.Direction.NO;
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        direction = Constants.Direction.UP;
                    }
                    else if (Input.GetKeyDown(KeyCode.S))
                    {
                        direction = Constants.Direction.DOWN;
                    }
                    else if (Input.GetKeyDown(KeyCode.A))
                    {
                        direction = Constants.Direction.LEFT;
                    }
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        direction = Constants.Direction.RIGHT;
                    }
                    
                    if (direction != Constants.Direction.NO)
                    {
                        var nextCell = GetNextCell(player2Tank.CurrentCell, direction);
                        //CheckValidDirection
                        if (IsValidCell(nextCell))
                        {
                            var move = UpdateMoveForPlayer(player2Tank, direction);
                            Player2Moves.Add(move);
                            player2Tank.lastDir = direction;
                            playerDone = true;
                        }
                    }
                }
                if (playerDone)
                {
                    playerDone = false;
                    CmdSetTurn();
                }
            }
        }
        [Command(requiresAuthority = false)]
        public void CmdSetTurn()
        {
            RpcSetTurn();
        }

        [ClientRpc]
        public void RpcSetTurn()
        {
            isOurTurn = !isOurTurn;
            if (isOurTurn)
            {
                yourTurn.SetActive(true);
            }
            else
            {
                yourTurn.SetActive(false);
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

        public Vector2 UpdateMoveForTank(TankP currentTank, TankP otherTank)
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
                //Debug.LogError(
                    //$"Your Direction Is Invalid Direction:{direction} CurrentCell:{currentTank.CurrentCell}");
            }

            return new Vector2(-1, -1);
        }
        public Vector2 UpdateMoveForPlayer(TankP currentTank, Constants.Direction direction)
        {
            
            var nextCell = GetNextCell(currentTank.CurrentCell, direction);
            //CheckValidDirection
            //if (IsValidCell(nextCell))
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

        public void UpdateEnemyMove()
        {
            if (!CanMove || IsGameOver)
            {
                //Debug.LogError("IsMoving:" + CanMove + " IsGameOver:" + IsGameOver);
                return;
            }

            Vector2 nextCell = Player.localPlayer.enemyInfo.CurrentCell;
            Constants.Direction direction = Player.localPlayer.enemyInfo.lastDirection;
            if (player1Tank.PlayerId == enemyId)
            {
                if (player1Tank.CurrentCell == nextCell)
                    return;
                player1Tank.SetCurrentCell(nextCell);
                var position = GetPosition(nextCell);
                CanMove = false;
                player1Tank.transform.DORotate(GetRotateByDirection(direction), 0.5f);
                player1Tank.transform.DOMove(new Vector3(position.x, position.y, 0), 1f).OnComplete(() =>
                {
                    CanMove = true;
                    OccupyPosition(nextCell, player1Tank.CurrentTank);
                });
            }
            if (player2Tank.PlayerId == enemyId)
            {
                if (player2Tank.CurrentCell == nextCell)
                    return;
                
                player2Tank.SetCurrentCell(nextCell);
                var position = GetPosition(nextCell);
                CanMove = false;
                player2Tank.transform.DORotate(GetRotateByDirection(direction), 0.5f);
                player2Tank.transform.DOMove(new Vector3(position.x, position.y, 0), 1f).OnComplete(() =>
                {
                    CanMove = true;
                    OccupyPosition(nextCell, player2Tank.CurrentTank);
                });
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

        private void Update()
        {
            if (Player.localPlayer && Player.localPlayer.hasEnemy)
            {
                if (Player.localPlayer.PlayerId == 1)
                {
                    nameP1.text = Player.localPlayer.username;
                    nameP2.text = Player.localPlayer.enemyInfo.username;
                    enemyId = 2;
                }
                else if (Player.localPlayer.PlayerId == 2)
                {
                    nameP2.text = Player.localPlayer.username;
                    nameP1.text = Player.localPlayer.enemyInfo.username;
                    enemyId = 1;
                }
                var winPlayer = CheckGameOver();
                if (winPlayer != Constants.GameResult.PLAYING)
                {
                    IsGameOver = true;
                    //Debug.LogError($" IsGameOver {IsGameOver} Result {winPlayer}");
                    switch (winPlayer)
                    {
                        case Constants.GameResult.PLAYER1_WIN:
                            TextScore.text = "Game Over Player " + nameP1.text + " Win";
                            break;
                        case Constants.GameResult.PLAYER2_WIN:
                            TextScore.text = "Game Over Player "  + nameP2.text + " Win";
                            break;
                        case Constants.GameResult.DRAW:
                            TextScore.text = "Game Over Draw";
                            break;
                    }
                    //Export Orginal Map and Moves of players and winner
                }

                if (isOurTurn)
                {
                    UpdateEnemyMove();
                    UpdateMove();
                }
            }
        }

        public void StartTurn()
        {
            yourTurn.SetActive(true);
            isOurTurn = true;
        }
        
        public void Start()
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

            //StartGame();
        }
        public void StartGame()
        {
            ScoreRed = 0;
            ScoreBlue = 0;
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
            pointP2.text = $"<color=blue>{ScoreBlue}</color>";
            pointP1.text = $"<color=red>{ScoreRed}</color>";
        }

        private TankP CreateTank(Constants.TankType tankType, int playerId)
        {
            var tank = Instantiate<TankP>(tankPrefab, new Vector3(0, 0, 0), Quaternion.identity,
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
            var x = 0;
            var y = 0;
            //generate random symmetric wall
            for (int i = 0; i < Config.WallCount / 2; i++)
            {
                // while (x == 0 && y == 0)
                // {
                //     x = Random.Range(0, Config.MapWidth / 2);
                //     y = Random.Range(0, Config.MapHeight / 2);
                // }
                y += 1;
                if (y%2==0)
                    x += 2;
                
                logicMap[x, y] = Constants.CellType.WALL;
                logicMap[Config.MapWidth - x - 1, Config.MapHeight - y - 1] = Constants.CellType.WALL;
            }

            for (int i = 0; i < Config.MapWidth; i++)
            {
                for (int j = 0; j < Config.MapHeight; j++)
                {
                    if (displayMap[i, j] == null)
                    {
                        var pos = GetPosition(new Vector2(i, j));
                        var cell = Instantiate(cellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity,
                            transform);
                        displayMap[i, j] = cell;
                    }
                }
            }
        }
    }
}