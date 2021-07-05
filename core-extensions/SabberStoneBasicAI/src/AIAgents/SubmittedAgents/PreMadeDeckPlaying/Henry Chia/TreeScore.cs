using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SabberStoneBasicAI.AIAgents.HenryChia
{
	public class TreeScore
	{
		public static string tree_node;
		public static double node_score;
		public static int digits = 1;
		public static double Node_Evaluation(int num_my_board, int num_op_board, int num_my_hand, int num_op_hand,
			int num_my_hero, int num_op_hero, int num_my_deck, int num_op_deck, int num_remaining_mana, int num_my_attackdamage,
			int num_op_attackdamage, int num_my_minionHealth, int num_op_minionHealth)		
		{
			Stack mystack = new Stack();
			Stack digit = new Stack();
			char[] b = new char[tree_node.Length];

			using (StringReader sr = new StringReader(tree_node))
			{
				
				try
				{
					sr.Read(b, 0, tree_node.Length);
				}
				catch (Exception e)
				{

				}

			
				double sum = 0;
				string q;
				string w;
				double digits_sum = 0;
				digit = null;
				for (int i = 0; i < b.Length; i++)
				{
					try
					{
						digits = 1;
						if (digit != null)
						{										
							digits_sum = double.Parse(digit.Pop().ToString()) + double.Parse(b[i].ToString());
							mystack.Push(digits_sum);
							digits_sum = 0;
							digit = null;
						}
						else
						{					
							if (b[i].Equals('a'))
							{

								mystack.Push(num_my_board);

							}
							else if (b[i].Equals('b'))
							{
								mystack.Push(num_op_board);

							}
							else if (b[i].Equals('c'))
							{
								mystack.Push(num_my_hand);

							}
							else if (b[i].Equals('d'))
							{
								mystack.Push(num_op_hand);

							}
							else if (b[i].Equals('e'))
							{
								mystack.Push(num_my_hero);

							}
							else if (b[i].Equals('f'))
							{
								mystack.Push(num_op_hero);

							}
							else if (b[i].Equals('g'))
							{
								mystack.Push(num_my_deck);

							}
							else if (b[i].Equals('h'))
							{
								mystack.Push(num_op_deck);

							}
							else if (b[i].Equals('i'))
							{
								mystack.Push(num_remaining_mana);

							}
							else if (b[i].Equals('j'))
							{
								mystack.Push(num_my_attackdamage);

							}
							else if (b[i].Equals('k'))
							{
								mystack.Push(num_op_attackdamage);

							}
							else if (b[i].Equals('l'))
							{
								mystack.Push(num_my_minionHealth);

							}
							else if (b[i].Equals('m'))
							{
								mystack.Push(num_op_minionHealth);

							}
							

							else if (b[i].Equals('A'))//+
							{

								q = mystack.Pop().ToString();
								w = mystack.Pop().ToString();
								sum = double.Parse(q) + double.Parse(w);
								mystack.Push(sum);

							}
							else if (b[i].Equals('B'))//-
							{

								q = mystack.Pop().ToString();
								w = mystack.Pop().ToString();

								sum = double.Parse(w) - double.Parse(q);
								mystack.Push(sum);

							}
							else if (b[i].Equals('C'))//*
							{

								q = mystack.Pop().ToString();
								w = mystack.Pop().ToString();
								sum = double.Parse(q) * double.Parse(w);
								mystack.Push(sum);

							}
							else if (b[i].Equals('D'))///
							{

								q = mystack.Pop().ToString();
								w = mystack.Pop().ToString();

								
								double p = double.Parse(q);
								if (p == 0)p = 1;
							
								sum = double.Parse(w) / p;
								mystack.Push(sum);

							}
							else if (b[i].Equals('E'))//^2
							{
								q = mystack.Pop().ToString();
								sum = double.Parse(q) * double.Parse(q);
								mystack.Push(sum);						
							}
							else if (b[i].Equals('F'))//sqrt
							{
								q = mystack.Pop().ToString();
								sum = Math.Sqrt(double.Parse(q));
								mystack.Push(sum);
	
							}
							else if (b[i].Equals('G'))//log
							{
								q = mystack.Pop().ToString();
								sum = Math.Log(double.Parse(q));
								mystack.Push(sum);
							
							}
							else if (b[i].Equals('H'))//exp
							{
								q = mystack.Pop().ToString();
								sum = Math.Exp(double.Parse(q));
								mystack.Push(sum);
							
							}

							if (!b[i].Equals('a') && !b[i].Equals('b') && !b[i].Equals('c') && !b[i].Equals('d')
								 && !b[i].Equals('e') && !b[i].Equals('f') && !b[i].Equals('g') && !b[i].Equals('h') && !b[i].Equals('i') &&
								!b[i].Equals('j') && !b[i].Equals('k') && !b[i].Equals('l') && !b[i].Equals('m') &&
								 
								 !b[i].Equals('A') && !b[i].Equals('B') && !b[i].Equals('C') && !b[i].Equals('D') &&
								 !b[i].Equals('E') && !b[i].Equals('F') && !b[i].Equals('G') && !b[i].Equals('H'))
							{
								mystack.Push(b[i]);

							}
							
						}
					}
					catch
					{
						continue;
					}


				}
				
				sum = Double.IsNaN(sum) ? Double.MaxValue : sum;
				node_score = sum;
			
			}

			return node_score;
		}
	}
}
