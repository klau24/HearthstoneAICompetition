import os
import argparse
import pandas as pd

# Usage: python3 resultsParser.py [path to results.txt]
def main(): 
    args = parse_args()
    f = open(args.file, 'r+')
    
    agents = []
    decks = []
    matches = []
    read_type = None
    
    for line in f:
        line = line.strip()
        if line == '':
            continue
        if line == 'Agents': 
            read_type = 'Agents'
            continue
        elif line == 'Decks':
            read_type = 'Decks'
            continue
        elif line == 'Match Results':
            read_type = 'Matches'
            continue
        
        if read_type == 'Agents': 
            agents.append(line)
        elif read_type == 'Decks':
            decks.append(line)
        elif read_type == 'Matches':
            matches.append([int(x) for x in line.split()[2:9]])
        else: 
            continue
        
    df = pd.DataFrame(columns=['a1', 'a2', 'd1', 'd2', 'wins', 'loses', 'ties', 'wr'])
        
    for match in matches: 
        a1 = match[0]
        a2 = match[1]
        d1 = match[2]
        d2 = match[3]
        wins = match[4]
        loses = match[5]
        ties = match[6]
       
        df = df.append(pd.Series([agents[a1], agents[a2], decks[d1], decks[d2], wins, loses, ties, wins/(wins+loses)], 
                                 index=df.columns), ignore_index=True)
    
    df.to_csv('results.csv')



def parse_args():
    parser = argparse.ArgumentParser(description='print results')
    parser.add_argument('file', nargs='?', default="/core-extensions/SabberStoneBasicAI/bin/Debug/netcoreapp2.1/results.txt")
    args = parser.parse_args()
    return args

if __name__ == '__main__':
    main()