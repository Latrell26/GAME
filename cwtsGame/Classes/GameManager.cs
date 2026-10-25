/*
 * Created by SharpDevelop.
 * User: cruzl
 * Date: 4/25/2026
 * Time: 4:31 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace cwtsGame
{
    public class GameManager
    {
        public int Score;
        public int Level;
        public int CorrectCount;
        public int TotalSorted;

        public GameManager()
        {
            Score = 0;
            Level = 1;
            CorrectCount = 0;
            TotalSorted = 0;
        }

        public void CorrectSort()
        {
            Score += 10 * Level;
            CorrectCount++;
            TotalSorted++;
        }

        public void WrongSort()
        {
            Score -= 5;
            if (Score < 0) Score = 0;
            TotalSorted++;
        }

        public void SpendScore(int amount)
        {
            Score -= amount;
            if (Score < 0) Score = 0;
        }
    }
}