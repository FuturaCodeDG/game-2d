using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* Este projeto faz parte do material utilizado no curso "Design de Games 2D e 3D", desenvolvido por Renan Gomes da Futura Code School
 * 
 * A musica utilizada nesse projeto se chama 'Epilogue' e foi disponibilizada pelo autor 'Creo' sob a licenca CC, podendo ser baixada em http://freemusicarchive.org/music/Creo/~/Epilogue_1849
 * O logotipo 'Fish out of Water!' eh utilizado apenas como exemplo. Ele pertence aa empresa Halfbrick e nao representa o nome deste projeto.
 */

public class Player : MonoBehaviour {

	public	Animator		bobAnimator;		// Controlador das animacoes
	public	float 			moveSpeed;			// Velocidade de movimento do personagem
	public	bool			isMoving;			// Identificador de movimento (verdadeiro ou falso)
	private	Vector3			bobScaleRight;		// Escala do personagem quando ele estiver virado para a direita
	private	Vector3			bobScaleLeft;		// Escala do personagem quando ele estiver virado para a esquerda
	public	Transform 		scoreHud;			// Elemento que mostra a pontuacao na tela
	public	Transform 		peixe;				// Peixinho
	private	int				score;				// Variavel que armazena a pontuacao (guarda o numero, nao mostra na tela)
	private	bool 			gameOver;			// Identifica se o jogo acabou (verdadeiro ou falso)
	private	bool			gameStarted;		// Identifica se o jogo comecou (verdadeiro ou falso)
	private	GameObject[]	gameOverHuds;		// Lista de objetos que possuem a tag "GameOver"
	private	GameObject[]	gameStartHuds;		// Lista de objetos que possuem a tag "GameStart"
	public	AudioSource		myAudio;			// Componente que emite os sons
	public	AudioClip		fishCollectSound;	// Som de quando o peixe eh coletado
	public	AudioClip		gameOverSound;		// Som de Game Over

	// A funcao padrao Start eh chamada sempre que o jogo eh iniciado
	void Start()
	{
		// A variavel bobScaleRight, do tipo Vector3, recebe os valores X, Y, Z da escala do objeto em questao (transform)
		bobScaleRight = transform.localScale;
		// A variavel bobScaleLeft, tambem do tipo Vector3, recebe os mesmos valores, mas com o X negativo
		bobScaleLeft = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);

		// Zera a pontuacao ao iniciar o jogo
		score = 0;

		// Identifica o objeto Peixe
		peixe = GameObject.Find ("Peixinho").transform;
		// Reseta a velocidade de queda do peixe
		peixe.GetComponent<Rigidbody2D> ().gravityScale = 1;

		// Reseta o game
		gameOver = false;
		gameStarted = false;
		Time.timeScale = 0;

		// Identifica os elementos da tela (HUD)
		gameStartHuds = GameObject.FindGameObjectsWithTag ("GameStart");
		gameOverHuds = GameObject.FindGameObjectsWithTag ("GameOver");
		// Desativa todos os elementos GameOver
		foreach (var item in gameOverHuds) {
			item.SetActive(false);
		}
		// Ativa todos os elementos GameStart
		foreach (var item in gameStartHuds) {
			item.SetActive(true);
		}

		// Identifica o emissor de sons
		myAudio = transform.GetComponent<AudioSource> ();
	}

	// A funcao padrao Update eh chamada constantemente, 1 vez por frame
	void Update () 
	{
		// Na tela inicial, aguarda o jogador apertar alguma tecla para iniciar o jogo
		if (Input.anyKeyDown) {
			Time.timeScale = 1;
			foreach (var item in gameStartHuds) {
				item.SetActive(false);
			}
		}

		// A variavel temporaria 'move' eh um Vector3 que recebe a informacao de entrada (input) do usuario para o eixo horizontal (x)
		Vector3 move = new Vector3 (Input.GetAxis ("Horizontal"), 0, 0);
		transform.position += move * moveSpeed * Time.deltaTime; // Altera a posicao do objeto de acordo com a variavel move, a velocidade do personagem e o tempo

		// Verifica movimentacao do personagem, passando como parametro a velocidade no eixo X
		MoveVerifier (move.x);

		// Limita a posicao do personagem
		MoveLimiter ();

		// Controla a pontuacao
		ScoreManager ();

		// Gira o peixinho 5 graus a cada frame
		peixe.Rotate (Vector3.forward, 5);

		// Verifica se o peixe passou da linha do chao e teleporta de volta
		if (peixe.position.y < -7) {
			if(!gameOver)
				TeleportFish (peixe);
			score--;
		}

		// Verifica se estah na tela GameOver
		if (gameOver) {
			if (Input.anyKeyDown) {
				Application.LoadLevel (0);
			}
		}

	}

	// Funcao que verifica a movimentacao do personagem e controla a animacao e direcao
	void MoveVerifier(float bobSpeed)
	{
		if (bobSpeed == 0) // Se bobSpeed for 0, ele esta parado
		{
			bobAnimator.SetBool("isWalking", false); // Altera parametro 'isWalking' para falso
		}
		else // Se nao, esta andando
		{
			bobAnimator.SetBool("isWalking", true); // Altera parametro 'isWalking' para verdadeiro

			if (bobSpeed > 0) // Se velocidade for positiva, esta andando para a direita
			{
				transform.localScale = bobScaleRight; // Altera a escala do personagem para a escala X positiva
			} 
			
			else // Se nao, esta andando para a esquerda
			{
				transform.localScale = bobScaleLeft; // Altera a escala do personagem para a escala X negativa
			}
		}
	}

	// Limita o movimento do personagem Bob para ele nao sair da tela
	void MoveLimiter()
	{
		if (transform.position.x > 7.5f) {
			transform.position = new Vector3 (7.5f, transform.position.y, transform.position.z);
			bobAnimator.SetBool ("isWalking", false); // Se encostar na borda da tela, para de andar
		} else if (transform.position.x < -7.5f) {
			transform.position = new Vector3 (-7.5f, transform.position.y, transform.position.z);
			bobAnimator.SetBool ("isWalking", false);
		}
	}

	// Atualiza a pontuacao
	void ScoreManager()
	{
		if (score < 0) { // Se a pontuacao for menor do que 0, chama a funcao GameOver() e impede que o score na tela fique negativo
			GameOver ();
			score = 0;
		}
		scoreHud.GetComponent<Text>().text = score.ToString(); // Define que o texto na tela seja igual aa variavel 'score', que precisa ser convertida para String para ser interpretada como texto em vez de numero.
	}

	// Gerencia quando o jogador perde
	void GameOver(){
		myAudio.PlayOneShot (gameOverSound); // Reproduz o som GameOver
		Time.timeScale = 0; // Para o tempo
		gameOver = true; // Define a variavel gameOver como verdadeira

		// Para cada objeto com a tag GameOver, ativa-o
		foreach (var item in gameOverHuds) {
			item.SetActive(true);
		}
	}

	// Detecta a colisao (se tocou no peixinho)
	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Food") {
			score++; // Aumenta a pontuacao
			peixe.GetComponent<Rigidbody2D>().gravityScale += 0.1f; // Aumenta a velocidade de queda do peixinho
			moveSpeed += 0.2f; // Aumenta a velocidade de movimento do personagem
			myAudio.PlayOneShot(fishCollectSound); // Toca o som de coleta
			TeleportFish(other.transform); // Teleporta o peixinho
		}
	}

	void TeleportFish(Transform fish){
		float randomX = Random.Range(-7,7); // Sorteia um valor aleatorio entre -7 e 7 (coordenada X)
		fish.position = new Vector3 (randomX, 6, 0); // Move o peixinho para a posicao sorteada
	}

	// Funcao que controla personagem alterando sua posicao de acordo com cada tecla pressionada
	void OldMoveControl() {
		/*
		// Esquerda
		if (Input.GetKey(KeyCode.LeftArrow)) {
			transform.position += Vector3.left * moveSpeed * Time.deltaTime;
		}

		// Direita
		if (Input.GetKey(KeyCode.RightArrow)) {
			transform.position += Vector3.right * moveSpeed * Time.deltaTime;
		}

		// Cima
		if (Input.GetKey(KeyCode.UpArrow)) {
			transform.position += Vector3.up * moveSpeed * Time.deltaTime;
		}

		// Baixo
		if (Input.GetKey(KeyCode.DownArrow)) {
			transform.position += Vector3.down * moveSpeed * Time.deltaTime;
		}
		*/
	}
}
