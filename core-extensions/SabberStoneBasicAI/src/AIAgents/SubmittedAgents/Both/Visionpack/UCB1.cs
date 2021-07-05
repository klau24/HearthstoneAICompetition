using System;
using System.Linq;


namespace SabberStoneBasicAI.AIAgents.Visionpack
{
	class UCB1
    {
		private static double c = 1.41; //more or less equal to sqrt(2)

		public static double ucb1Value(int totalNodeVisits, int nodeStateActionUses, double actionStateScore)//N(s), N(s,a), Q(s, a)
		{
			//Console.WriteLine("actionStateScore: " + actionStateScore + " totalNodeVisits: " + totalNodeVisits + "  nodeStateActionUses: " + nodeStateActionUses);
			if (nodeStateActionUses == 0)//totalNodeVisits == 0)
			{
				//Console.WriteLine("Max: " + Int32.MaxValue);
				return Int32.MaxValue;  
			}
			
			return ((double)actionStateScore + c * Math.Sqrt(Math.Log(totalNodeVisits) / (double)nodeStateActionUses));  //Q(s, a) + C*SQRT(ln(N(s))/N(s, a))
		}

		public static MyMCTS findBestNodeWithUCB1(MyMCTS node)
		{
			int parentVisit = node.visitCounter;
			double ucb1 = Double.MinValue;
			int counter = -1;
			int pos = -1;
			if(node.scores == null)
			{
				Console.WriteLine("No scores!   Depth: "+node.depth);
			}
			foreach (int score in node.scores)
			{
				counter++;
				double temp = ucb1Value(node.visitCounter, node.actionfromThisStateCounter[counter], score);				
				if (temp > ucb1)
				{
					ucb1 = temp;
					pos = counter;					
				}
				//Console.WriteLine("UCB1-Value:  " + temp + "   Position: " + counter + "  Action: " + node.children.ElementAt(counter).lastTask+"  BestScore: "+ node.children.ElementAt(counter).bestScore);
			}
			//Console.WriteLine("---------------------UCB1 Done-----Pos: "+pos+" last Task: "+ node.children.ElementAt(pos).lastTask);
			node.lastTaskPos = pos;
			return node.children.ElementAt(pos);
		}
	}
}
