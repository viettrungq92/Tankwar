using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Topebox.Tankwars
{
    [Serializable]
    public class Tank : MonoBehaviour
    {
        public Constants.TankType CurrentTank = Constants.TankType.RED;
        public SpriteRenderer SpriteRenderer;
        public Sprite RedSprite;
        public Sprite BlueSprite;
        public int PlayerId;
        public Vector2 CurrentCell;

        public void SetType(Constants.TankType tankType)
        {
            CurrentTank = tankType;
            switch (CurrentTank)
            {
                case Constants.TankType.RED:
                    SpriteRenderer.sprite = RedSprite;
                    break;
                case Constants.TankType.BLUE:
                    SpriteRenderer.sprite = BlueSprite;
                    break;
            }
        }

        public void SetId(int playerId)
        {
            PlayerId = playerId;
        }

        public void SetCurrentCell(Vector2 pos)
        {
            CurrentCell = pos;
        }

        public Constants.Direction GetNextMove(GameState game)
        {
            var myPosition = CurrentCell;

            var availableMove = new List<Constants.Direction>();
            var upCell = game.GetNextCell(myPosition, Constants.Direction.UP);
            if (game.IsValidCell(upCell))
            {
                availableMove.Add(Constants.Direction.UP);
            }

            var downCell = game.GetNextCell(myPosition, Constants.Direction.DOWN);
            if (game.IsValidCell(downCell))
            {
                availableMove.Add(Constants.Direction.DOWN);
            }

            var leftCell = game.GetNextCell(myPosition, Constants.Direction.LEFT);
            if (game.IsValidCell(leftCell))
            {
                availableMove.Add(Constants.Direction.LEFT);
            }

            var rightCell = game.GetNextCell(myPosition, Constants.Direction.RIGHT);
            if (game.IsValidCell(rightCell))
            {
                availableMove.Add(Constants.Direction.RIGHT);
            }

            if (availableMove.Count == 0) //if have no available move, return DOWN
                return Constants.Direction.DOWN;
            
            if (availableMove.Count == 1) //if have 1 available move, return it
                return availableMove[0];

            //TODO: Your logic here
            return availableMove[Random.Range(0, availableMove.Count)]; //temp return random move
        }

        public List<Constants.Direction> GetValidMove(GameState game)
        {
            var myPosition = CurrentCell;

            var availableMove = new List<Constants.Direction>();
            var upCell = game.GetNextCell(myPosition, Constants.Direction.UP);
            if (game.IsValidCell(upCell))
            {
                availableMove.Add(Constants.Direction.UP);
            }

            var downCell = game.GetNextCell(myPosition, Constants.Direction.DOWN);
            if (game.IsValidCell(downCell))
            {
                availableMove.Add(Constants.Direction.DOWN);
            }

            var leftCell = game.GetNextCell(myPosition, Constants.Direction.LEFT);
            if (game.IsValidCell(leftCell))
            {
                availableMove.Add(Constants.Direction.LEFT);
            }

            var rightCell = game.GetNextCell(myPosition, Constants.Direction.RIGHT);
            if (game.IsValidCell(rightCell))
            {
                availableMove.Add(Constants.Direction.RIGHT);
            }

            return availableMove;
        }

        public (int, Constants.Direction) Minimax(GameState game, bool maximizing)
        {
            var winPlayer = game.CheckGameOver();
            if (winPlayer == Constants.GameResult.PLAYER1_WIN)
                return (1, Constants.Direction.NO);
            else if (winPlayer == Constants.GameResult.PLAYER2_WIN)
                return (-1, Constants.Direction.NO);
            else if (winPlayer == Constants.GameResult.DRAW)
                return (0, Constants.Direction.NO);
            if (maximizing)
            {
                int max_eval = -100;
                Constants.Direction best_move = Constants.Direction.NO;
                List<Constants.Direction> availableMove = GetValidMove(game);
                foreach (Constants.Direction move in availableMove)
                {
                    GameState temp_game = (GameState)game.DeepCopy();
                    var mo = temp_game.UpdateMoveForAI(temp_game.player1Tank, move);
                    temp_game.Player1Moves.Add(mo);
                    temp_game.CurrentPlayer = temp_game.player2Tank.PlayerId;
                    if (move != Constants.Direction.NO)
                        Debug.Log("move = " + "dif");
                    int eval = (int)((ITuple)Minimax(temp_game, false))[0];
                    if (eval > max_eval)
                    {
                        max_eval = eval;
                        best_move = move;
                    }
                }
                if (best_move != Constants.Direction.NO)
                    Debug.Log("move = " + "dif");
                return (max_eval, best_move);
            }
            else if (!maximizing)
            {
                int min_eval = 100;
                Constants.Direction best_move = Constants.Direction.NO;
                List<Constants.Direction> availableMove = GetValidMove(game);
                foreach (Constants.Direction move in availableMove)
                {
                    GameState temp_game = (GameState)game.DeepCopy();
                    var mo = temp_game.UpdateMoveForAI(temp_game.player2Tank, move);
                    temp_game.Player2Moves.Add(mo);
                    temp_game.CurrentPlayer = temp_game.player1Tank.PlayerId;
                    int eval = (int)((ITuple)Minimax(temp_game, true))[0];
                    if (eval < min_eval)
                    {
                        min_eval = eval;
                        best_move = move;
                    }
                }

                return (min_eval, best_move);
            }
            else
            {
                return (0, Constants.Direction.NO);
            }
        }
    }
}