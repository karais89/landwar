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

    private void on_click(short index)
    {
        throw new NotImplementedException();
    }
}
