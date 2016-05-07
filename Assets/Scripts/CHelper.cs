using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CHelper
{
	/*
		■ 이동 가능한 셀 찾기

		이동 규칙은 아주 간단합니다.
		현재 자신의 위치를 기준으로 상하좌우 대각선 한칸은 복제 두칸은 이동입니다.

		말로는 이렇게 간단한것이 프로그램 코드로 작성하려고 하면 순간 숨이 탁 막히죠.
		베이직으로 구구단을 작성하며 기뻐하던 시절에는 저런것 구현하느라 며칠씩
		밤을 새기도 했었습니다.
		지금에 와서 조금 노하우란게 생겼다면 일단 작은 문제로 쪼개는 겁니다.
		그리고 각 단어나 문제등을 명확하게 표현하는것이 중요합니다.
		논리적이라고 해야하나요? 컴퓨터는 확실한걸 좋아하니 말이죠.
	*/
	
	/*
	☞  상대방 셀 찾기

		내가 이동할 셀을 찾는데 성공했으면 다음으로 상대방 셀을 찾아야 합니다.
		이동범위 내에 상대방 캐릭터가 존재한다면 그곳은 갈 수 없도록 처리해야 하기 때문이죠.
		이것도 코딩하기 전에 어떻게 구현할지 생각해 봅시다.
		제일 먼저 떠오르는 방법은 일단 이동가능한 범위 내의 셀을 모두 구합니다.
		그리고 그 범위내의 셀들중에 상대방의 캐릭터가 존재한다면 해당셀을 리스트에서 제거해 줍니다.
		참~ 쉽죠? 코드로 구현해볼까요?
	*/
	public static List<short> find_available_cells(short basis_cell, List<short> total_cells, List<CPlayer> players)
	{
		/*
		*참고 : ForEach는 c#에서 리스트의 모든 요소들을 돌면서 처리하는 명령어 입니다.
		for (int i=0; i<list.count; ++i) 이런코드 많이 보셨죠?
		하는일은 비슷합니다. 코딩하기 더 편해서 저는 자주 써요.^^

		=> 라는 기호가 보이실 텐데요, c#에서 람다식(?) 이라고 불리우는 것인데,
		왼족이 변수, 오른쪽이 수행문(?) 정도로 생각하면 됩니다.

		옛날에 C/C++로 코딩할때는 거의 함수로 도배하고 다녔었는데요,
		C#을 쓰니 람다식이다 익명함수다 델리게이트다 이런것들에 익숙해지니
		코딩이 즐겁더라고요.^^ 보기에도 깔끔해지고요.
		아직 모르시는 분들은 위 키워드로 자료를 찾아보시고 한번 써보시길 권해드립니다.
		*/
		
		// 2칸 이내 범위의 모든 셀을 구합니다.
		List<short> targets = find_neighbor_cells(basis_cell, total_cells, 2);
		
		// 리스트중에 플레이어의 캐릭터와 겹치는곳은 모두 제거해 버립니다.
		players.ForEach(obj => {
			targets.RemoveAll(number => obj.cell_indexes.Exists(cell => cell == number));
		});		
		
		// string debug = basis_cell.ToString() + " => ";
		// targets.ForEach(obj => {
		// 	debug += string.Format("{0}", obj);
		// });
		// Debug.Log(debug);
		
		return targets;
	}	
	
	/*
	☞  상하좌우 대각선 셀 구하기
		위에서 이동규칙은 상하좌우 대각선 두칸까지라고 했습니다.
		두칸은 이동, 한칸은 복제니까 최대 두칸범위 까지는 움직일 수 있는것이죠.

		그럼 특정 셀에서 두칸범위의 셀들을 구하는 부분을 작성해 보겠습니다.
		함수 이름은 이웃셀이라는 개념으로 find_neighbor_cells라고 해보죠.
		기준되는 셀과, 전체 게임 보드판을 들고있는 리스트값을 파라미터로 받습니다.
		그리고 얼마만큼 떨어진 칸을 구할것인지도 파라미터로 넣어줍니다.
		(최대 두칸이라고 정해져있지만 하드코딩 하지 말고 이렇게 파라미터로 넣어주면
		n칸에 대해서 모두 구할 수 있으니 이후로도 훨씬 더 편하겠죠?
		이왕 하는거 편하게~ 편하게~^^)
		
		자, 이렇게 해서 특정위치로 부터 이동 가능한 셀을 찾는데 성공했습니다.
		뭐 이부분도 구현하는 사람에 따라서 천차만별이겠지요.
		컴퓨터가 알아들을 수 있도록 작성하면 되는겁니다.^^
		더불어 유지보수를 위해서라면 사람이 이해하기도 쉬워야합니다.
	*/
	// Find neighbor cells of this cell.
	// targets : total game board.
	public static List<short> find_neighbor_cells(short basis_cell, List<short> targets, short gap)
	{
		// cell -> x, y position 으로 변환합니다.
		Vector2 pos = convert_to_position(basis_cell);
		
		// 전체 모드판 중에서 gap이내에 존재하는 셀들만 find해서 리턴합니다.
		return targets.FindAll(obj => get_distance(pos, convert_to_position(obj)) <= gap);
	}
	
	// cell 인덱스를 넣으면 둘 사이의 거리값을 리턴해 줍니다.
	// 한칸이 차이나면 1, 두칸이 차이나면 2
	public static byte get_distance(short from, short to)
	{
		return get_distance(convert_to_position(from), convert_to_position(to));
	}
	
	// 함수 이름은 같지만 피라미터 타입이 다르기 때문에 위와는 다른 함수이죠
	public static byte get_distance(Vector2 pos1, Vector2 pos2)
	{
		Vector2 distance = (pos1 - pos2);
		short x = (short)Mathf.Abs(distance.x);
		short y = (short)Mathf.Abs(distance.y);
		
		// x, y중 큰 값이 실제 거리를 뜻합니다.
		// 직접 연습장에 좌표를 적어서 계산해보면 이해가 쉬울거예요.
		return (byte)Mathf.Max(x,y);
	}
	
	/*
	☞ 현재 자신의 위치 구하기

	셀의 위치를 표현하는 방법은 두가지가 있습니다.
	하나는 x,y 좌표로 표시하는것(2차원 배열을 생각해 보세요).
	또다른 하나는 0,1,2... 인덱스 순서로 표시하는것.
	저는 기본적으로 인덱스 방식을 사용하면서,
	x,y좌표로도 표현할 수 있도록 변환하는 함수를 만들것입니다.

	아래 세 함수는 셀의 인덱스값을 파라미터로 받아 x,y좌표로 변환하는데 사용되는 함수들입니다.
	
	* 참고:가로,세로가 8칸으로 똑같은 정사각형 모양이기 때문에 calc_row, calc_col
	  함수에서 똑같이 COL_COUNT라는 값을 참조했습니다.
	  정사각형이 아니라면 적절하게 바꿔줘야 겠지요?
	*/
	// calculate vertical index
	public static short calc_row(short cell)
	{
		return (short)(cell / CBattleRoom.COL_COUNT);
	}
	
	// calculate horizontal index
	public static short calc_col(short cell)
	{
		return (short)(cell % CBattleRoom.COL_COUNT);
	}
	
	// convert cell number to (x,y) position
	public static Vector2 convert_to_position(short cell)
	{
		return new Vector2(calc_row(cell), calc_col(cell));
	}
}
