/*
	세균전 게임의 규칙은 아주 간단합니다.
	자신의 셀을 하나 선택한 뒤,
	한칸은 복제,
	두칸은 이동의 규칙으로 캐릭터를 움직일 수 있습니다.
	움직인 곳 주위 8칸에 상대방의 캐릭터가 있다면
	전염시켜서 잡아 먹을 수 있습니다.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CBattleRoom : MonoBehaviour 
{
    public static readonly int COL_COUNT = 7;

	// 원작자는 변수명 snake 표기법을 사용함 - c++에서 부스트에서 이런식으로 사용하는 듯.
	// c# 에서는 카멜 표기법을 주로 사용하는 것 같다. 
	List<short> table_board;
	List<short> available_attack_cells;
	Texture graycell;
	Texture focus_cell;
	GUISkin blank_skin;
	Texture blank_image;
	Texture game_board;
	Texture background;
	List<Texture> img_players;
	Texture button_playagain;
	List<CPlayer> players;
	List<short> board;
	
	int current_player_index;
	int step;
	
	void Awake()	
	{
		// Awake 함수는 해당 스크립트가 작동될 때 제일 먼저 호출되는것을 보장합니다.
		// Start 함수는 렌더링 직전에 호출됩니다.
		// Awak함수와, Start함수는 최초 한번만 호출됩니다.
		// 게임 오브젝트를 비활성화 한 뒤 다시 활성화 시켜도 이 두 함수는 수행되지 않습니다.
		// 여기에는 리소스 로드처럼 정말 한번만 수행되야 할 코드를 넣는것이 좋습니다.
		// 게임을 재시작하여 변수값을 초기상태로 만들어야 하는 등의 코드는 clear나 reset같은 함수들을 만들어서
		// 로직 흐름에 맞게 호출해주는 방식을 써야 합니다.
		
		// Resources 로드 작업
		this.table_board = new List<short>();
		this.available_attack_cells = new List<short>();
		this.graycell = Resources.Load("images/graycell") as Texture;
		this.focus_cell = Resources.Load("images/border") as Texture;
		
		this.blank_skin = Resources.Load("blank_skin") as GUISkin;			
		this.blank_image = Resources.Load("images/blank") as Texture;
		this.game_board = Resources.Load("images/gameboard") as Texture;
		this.background = Resources.Load("images/gameboard_bg") as Texture;
		this.img_players = new List<Texture>();
		this.img_players.Add(Resources.Load("images/blue") as Texture);
		this.img_players.Add(Resources.Load("images/red") as Texture);
		
		this.button_playagain = Resources.Load("images/playagain") as Texture;
		
		this.players = new List<CPlayer>();
		for(byte i = 0; i < 2; ++i)
		{
			GameObject obj = new GameObject(string.Format("player{0}", i));
			CPlayer player = obj.AddComponent<CPlayer>();
			player.initialize(i);	
			this.players.Add(player);					
		} 	
		
		this.board = new List<short>();
		
		reset();
	}

    private void reset()
    {
        // throw new NotImplementedException();
    }
	
	// 화면에 그려주기.
	float ratio = 1.0f;
    void OnGUI()
	{
		// OnGUI함수는 유니티 엔진 내부에서 호출되는 함수입니다.
		// 매 프레임(혹은 더 자주) 호출되는 함수이며 모든 draw코드가 들어가게 됩니다.
		// 성능을 위해서는 OnGUI함수는 되도록 쓰지 않는 것이 좋습니다
		// 얼마 되지 않는 코드로도 DrawCall이 쭉쭉 올라가는 모습을 볼 수 있기 때문이죠
		// 이 강좌에서는 게임 로직에 집중하기 위하여 그냥 순수한 유니티의 기능 그대로를 사용해씁니다.		
		
		// 화면의 가로크기를 기준으로 비율을 정합니다.
		ratio = Screen.width / 800.0f;
		
		GUI.skin = this.blank_skin;
		
		draw_board();
		
		// 재시작 버튼에 대한 처리입니다.
		if(GUI.Button(new Rect(10, 10, 80 * ratio, 80 * ratio), this.button_playagain ))
		{
			StopAllCoroutines();
			reset();			
		}
	}

	// 800*480픽셀의 해상도를 기본으로 잡습니다(갤럭시S 사이즈).
	// 현재 화면의 사이즈를 구해서 기본값보다 큰지, 작은지 비율을 구합니다.
	// 이 비율에 맞게 버튼, 이미지들의 width, height를 조절해 줍니다.
	// 해상도 비율이 기본으로 잡은 800*480의 비율과 약간 다를 경우 상,하에 여백이 생길 수 있습니다.
	// 이런 여백은 까맣게 처리하거나, 배경 이미지를 쭉 늘리는 방식으로 처리할 수 있겠지요.
    private void draw_board()
    {
			//----------------------------------------------------------------------------------
			//----- 다양한 해상도를 지원하기 위해 설정하는 부분입니다.
			//----- 게임 실행중 해상도가 변하지 않는다면 미리 계산해 놓는것이 좋겠지요.
		//*****************************************************
		float scaled_height = 480.0f * ratio;
		float gap_height = Screen.height - scaled_height;
		
		float outline_left = 0;
		float outline_top = gap_height * 0.5f;
		float outline_width = Screen.width;
		float outline_height = scaled_height;
			
		float hor_center = outline_width * 0.5f;
		float ver_center = outline_height * 0.5f;
		//*****************************************************
		
		
		GUI.BeginGroup(new Rect(0, 0, outline_width, Screen.height));
		
		// 배경 이미지는 화면에 가득차게 그려줍니다.
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.background);
		
		// 게임 보드판은 화면 가운데로 오도록 그려줍니다.
		GUI.DrawTexture(new Rect(0, outline_top, outline_width, outline_height), this.game_board);

		
		// 셀 하나의 너비는 기본 60픽셀에 비율을 곱한 값입니다.
		int width = (int)(60 * ratio);
		int celloutline_width = width * CBattleRoom.COL_COUNT;
		float half_celloutline_width = celloutline_width * 0.5f;
		
		// BeginGroup - EndGroup으로 가상의 영역을 설정하여,
		// 그 안에 그려지는 것들은 (0, 0) 에서 시작할 수 있도록 좌표 계산을 단순화 하였습니다.
		GUI.BeginGroup(new Rect(hor_center - half_celloutline_width,
			ver_center - half_celloutline_width + outline_top, celloutline_width, celloutline_width));
		
		List<int> current_turn = new List<int>();
		short index = 0;
		for(int row = 0; row < CBattleRoom.COL_COUNT; ++row)
		{
			int gap_y = 0;
			for(int col = 0; col < CBattleRoom.COL_COUNT; ++col)
			{
				int gap_x = 0;
				
				// 셀 하나하나는 버튼으로 되어 있습니다.
				// 클릭이나 터치 이벤트를 받기 위해서죠.
				Rect cell_rect = new Rect(col * width + gap_x, row * width + gap_y, width, width);
				if(GUI.Button(cell_rect, ""))
				{
					// 셀을 클릭했을 때 호출되는 함수입니다.
					on_click(index);
				}
				
				
				if(this.board[index] != short.MaxValue)
				{
					int player_index = this.board[index];
					GUI.DrawTexture(cell_rect, this.img_players[player_index]);
									
					if(this.current_player_index == player_index)
					{
						GUI.DrawTexture(cell_rect, this.focus_cell);
					}
				}
				
				if(this.available_attack_cells.Contains(index))
				{
					// 공격 가능한 셀은 점선으로 돋보이도록 표시해줍니다.
					GUI.DrawTexture(cell_rect, this.focus_cell);
				}
						
				++index;
			}
		}		

		// BeginGroup - EndGroup은 항상 쌍이 맞아야 합니다.
		GUI.EndGroup();			
		GUI.EndGroup();
    }

	short selected_cell = short.MaxValue;
    private void on_click(short cell)
    {
		// 일반적인 턴제 퍼즐 게임에서 단계를 나눠 처리하는 방법은
		// 이런식으로 각 단계에 따른 변수 값을 바꿔서 처리합니다.
		
		// 0일때는 무슨무슨 작업
		// 1일때는 무슨무슨 작업
		// 이런식으로 말이죠.
		
		// 조금 복잡하고 세련되게 간다면 FSM같은것을 써서 만들 수도 있겠지요.
		// 여기서는 간단한 게임이므로 그냥 switch와 정수값으로 구분하였습니다.
		// (하지만 테스트 프로그램이라고 정수값을 그냥 쓰기보다는 enum으로 정의해서 쓰는 것이 좋습니다.)
		
		// Debug.Log(cell);
		
		switch (this.step)
		{
			case 0:
				// 첫번째 단계 : 자신의 셀을 하나 선택한 이후의 처리 부분 입니다.
				// 올바른 셀을 선택했는지 체크해 줘야 겠죠?
				if(validate_begin_cell(cell))
				{
					this.selected_cell = cell;
					Debug.Log("go to step2");
					
					// 다음에 또 클릭 이벤트가 발생하면 step 1로 처리할 수 있도록 설정 해줍니다.
					this.step = -1;
					
					// 이동 가능한 셀을 구해봅니다.
					refresh_available_cells(this.selected_cell);
				}
				break;
			
			case 1:
				{
					// 두번째 단계 : 이동한 셀을 선택한 이후의 처리부분 입니다.
					// 자신의 셀을 다시 선택한다면 이동 범위를 재설정 해 줍니다.
					if(this.players[this.current_player_index].cell_indexes.Exists(obj => obj == cell))
					{
						this.selected_cell = cell;
						refresh_available_cells(this.selected_cell);
						break;
					}
					
					// 다른 플레이어가 있는 셀은 선택할 수 없습니다.
					foreach(CPlayer player in this.players)
					{
						if(player.cell_indexes.Exists(obj => obj == cell))
						{
							return;
						}
					}	
					
					this.step = 2;
					
					// 공격을 시작합니다.
					StartCoroutine(on_selected_cell_to_attack(cell));			
				}
				break;
				
			case 2:
				// playin AI now
				break;			
		}		
    }

    private IEnumerator on_selected_cell_to_attack(short cell)
    {
        throw new NotImplementedException();
    }

    private void refresh_available_cells(short selected_cell)
    {
        throw new NotImplementedException();
    }


    // 올바른 셀을 체크했는지 여부.
    private bool validate_begin_cell(short cell)
    {
        throw new NotImplementedException();
    }
	
	/*
	★ 여기서 처음 등장하는 특이한 함수가 하나 있습니다.
	그것은 바로 StartCoroutine
	유니티 엔진에서는 코루틴이라는 아주 재미있는 기능을 제공합니다.

	보통 특정한 시나리오를 기술하는데 아주 유용하게 쓸 수 있는 기능이죠.
	이 게임을 예로 든다면

	단계1. 캐릭터 이동
	단계2. 이동하는 모션 재생
	단계3. 주위의 적들을 하나씩 잡아먹는 이펙트 출력
	단계4. 턴 종료.

	이렇게 순차적으로 공격 프로세스가 이루어 질 텐데 
	각 단계마다 적절한 대기 시간이 필요하게 됩니다.
	이동은 1초동안, 적들을 잡아먹는 이펙트는 0.5초씩 이런식으로 말이죠.

	전통적인 방식으로 코딩한다면 타이머와 콜백함수를 써서 만들 수 있습니다.
	하지만 이 방식의 단점은 잘 알고 계시듯이 
	로직의 분산,
	변수 공유의 까다로움(멤버 변수나, 제3의 객체를 사용해야 함)
	코딩의 난잡함
	디버깅의 귀찮음 등이 있습니다.

	같은 로직을 코루틴으로 구성한다면 이 모든 단계들이
	하나의 함수 안에 들어가게 됩니다.
	즉, 관련된 로직을 분산시키지 않고 모아놓을 수 있으며,
	변수 공유도 지역변수 쓰듯이 쓰면 되는 것이지요.
	예를들면

	while(액션이 끝날때까지)
	{
		플레이어 이동
		1초간 대기

		상대방 캐릭터 잡아먹기
		0.5초간 대기
	}

	2초간 대기
	턴 종료


	이런식으로 대본 쓰듯이 쭉 나열하는것이 가능해 집니다.
	이 게임에서 사용된 실제 코드를 보겠습니다.
		
	yield문을 만나면 그 즉시 함수의 수행이 멈추고 제어권을 반환하게 됩니다.
	마치 함수를 빠져나온 것처럼 말이죠!

	그리고 지정된 시간이 지나면 다시 yield다음 라인부터 수행이 이루어 집니다.
	마치 타이머와 콜백함수를 써서 임의의 코드를 실행하는것과 같다고 할 수 있지요.

	시간지연이 필요한 연출효과를 나타낼때 아주 유용하게 쓰일 수 있는 기능입니다.
	제가 예전에 작업했던 프로젝트에서도 많이 사용하여 재미를 쏠쏠하게 봤습니다.

	● 레이싱 게임에서 피트인을 하는 장면을 구현하는 코드
	IEnumerator run_pitin()  
	{  
		자동차 입장 모션 재생()  
		yield return new WaitForSeconds(3.0f);  
	
		피트인 에니메이션 연출()  
		yield return new WaitForSeconds(7.0f);  
	
		퇴장 모션 재생()
		yield return new WaitForSeconds(2.0f);  
	}  	
	*/
	IEnumerator reproduce(short cell)
	{
		CPlayer current_player = this.players[this.current_player_index];
		CPlayer other_player = this.players.Find(obj => obj.player_index != this.current_player_index);
		
		clear_available_attacking_cells();	
		
		// 0.5초간 대기 후 다음라인 부터 이어서 수행.
		yield return new WaitForSeconds(0.5f);
		
		// 이동한 셀을 플레이어의 셀로 넣어준다.
		this.board[cell] = current_player.player_index;
		current_player.add(cell);
		
		// 0.5초간 대기 후 다음라인부터 이어서 수행
		yield return new WaitForSeconds(0.5f);
		
		// 근처에 있는 상대방 셀을 구합니다.
		List<short> neighbors = CHelper.find_neighbor_cells(cell, other_player.cell_indexes, 1);
		foreach(short obj in neighbors)
		{
			// 상대방 셀을 하나씩 나의 셀로 만들어 줍니다.
			this.board[obj] = current_player.player_index;
			current_player.add(obj);
			
			other_player.remove(obj);
			
			// 하나를 먹었으면 0.2초간 대기 후 다음으로 진행합니다.
			yield return new WaitForSeconds(0.2f);
		}		
	}

    private void clear_available_attacking_cells()
    {
        throw new NotImplementedException();
    }
}
