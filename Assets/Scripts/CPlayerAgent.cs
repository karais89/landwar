using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CellInfo
{
	public int score;
	public short from_cell;
	public short to_cell;
}

public class CPlayerAgent
{
	/*
	우리가 만들 인공지능 클래스 입니다.
	사전을 찾아보니 Agent라는 단어가 잘 어울리는듯 하여(대리인) 이렇게 지었습니다.

	이 클래스의 멤버함수로 run 이라는 것을 만들어서
	딱~ 호출만 하면 알아서 자기 먹을곳을 찾고, 알아서 이동하게끔 만들겁니다.
	물론 우리가 다 직접 코딩해줘야 하는것이지요.
	컴퓨터는 스스로 알아먹지 못합니다.ㅠ_ㅠ

	인간 vs 컴퓨터 체스 경기 같은것도 사실은 우리같은(!) 프로그래머들이
	프로그램 해 넣은 컴퓨터와 경기를 하는것이지요.
	
	일단 코딩하기 전에 기본로직을 세워 봅시다.
		
		1) 자신의 캐릭터가 존재하는 셀을 하나 선택합니다.
		2) 그 위치에서 이동 가능한 범위의 셀을 모두 구합니다.
		3) 모든 셀을 돌면서 점수를 구합니다.
			복제는 +1점, 이동은 +0점.
			복제 또는 이동한 위치에서 공격을 시도해본 뒤
			나의 셀로 만들 수 있는곳은 모두 +1점씩 추가.
		4) 자신의 모든 캐릭터의 셀을 대상으로 1~3을 반복합니다.
		5) 점수가 가장 많은 곳을 선택합니다.

	단순한 규칙으로 이렇게 세울 수 있겠네요.
	이 강좌에서는 여기까지만 구현해보기로 하겠습니다.

	사실 세균전 게임을 하다보면 무조건 복제하는것만이 능사가 아니라는것을 알게 됩니다.
	그만큼 상대방에게 잡아먹힐 셀도 많아질 수 있기 때문이죠.
	때로는 이동을, 때로는 복제를 적절히 섞어가며 하는것이 승리의 지름길 입니다.

	바로 코드를 보여드리겠습니다.
	게임의 다른 로직과 어떻게 융합되는지는 차차 알려드릴께요.^^
	*/
	
	public CellInfo run(List<short> board, List<CPlayer> players, List<short> attacker_cells, List<short> victim_cells)
	{
		List<CellInfo> cell_scores = new List<CellInfo>();
		int total_best_score = 0;
		attacker_cells.ForEach(cell =>
		{
			int best_score = 0;
			short cell_the_best = 0;
			List<short> available_cells = CHelper.find_available_cells(cell, board, players);
			available_cells.ForEach(to_cell =>
			{
				// simulate! 가장 중요한 부분입니다.
				int score = calc_score(cell, to_cell, victim_cells);
				if(best_score < score)
				{
					cell_the_best = to_cell;
					best_score = score;
				}	
			});
			
			if(total_best_score < best_score)
			{
				total_best_score = best_score;				
			}			
			
			CellInfo info = new CellInfo();
			info.score = best_score;
			info.from_cell = cell;
			info.to_cell = cell_the_best;
			cell_scores.Add(info);
		});
		
		// 최고점수에 해당하는 데이터들을 모아서 그중 랜덤으로 하나를 선택합니다.
		List<CellInfo> top_scores = cell_scores.FindAll(info => info.score == total_best_score);
		System.Random rnd = new System.Random();
		int index = rnd.Next(0, top_scores.Count);
		return top_scores[index];
		
		// cell_scores.Sort(delegate(CellInfo left, CellInfo right) { return right.score.CompareTo(left.score);});
		// return cell_scores[0];		
	}
	
	int calc_score(short from_cell, short to_cell, List<short> victim_cells)
	{
		int score = 0;
		
		// 1. Calculate move score. clone = 1, move = 0 복제는 1점 이동은 0점으로 계산.
		short distance = CHelper.get_distance(from_cell, to_cell);
		if(distance <= 1)
		{
			score = 1;
		}
		
		// 2. Calculate fighting score. 공격 점수를 계산합니다.
		int fighting_score = calc_cellcount_to_eat(to_cell, victim_cells);
		
		return score + fighting_score;
	}
	
	int calc_cellcount_to_eat(short cell, List<short> victim_cells)
	{
		// 한칸 이웃한 셀들은 다 잡어먹을 수 있다고 보면 되겠네요. 셀 개수만큼 점수에 더해줍니다.
		List<short> cells_to_eat = CHelper.find_neighbor_cells(cell, victim_cells, 1);
		return cells_to_eat.Count;		
	}	
}
