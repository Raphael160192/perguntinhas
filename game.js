// Dados iniciais
const gameData = {
    players: [
        { name: "Jogador 1", score: 12, totens: 4 },
        { name: "Jogador 2", score: 12, totens: 4 }
    ],
    questions: [
        // Harry Potter
        { question: "Qual é o nome da escola de magia frequentada por Harry Potter?", options: ["Durmstrang", "Hogwarts", "Beauxbatons", "Ilvermorny"], answer: 1, level: 1, theme: "Harry Potter" },
        { question: "Quem é o diretor de Hogwarts na maior parte da série?", options: ["Severus Snape", "Minerva McGonagall", "Alvo Dumbledore", "Hagrid"], answer: 2, level: 1, theme: "Harry Potter" },
        { question: "Qual é o nome do elfo doméstico de Harry Potter?", options: ["Dobby", "Kreacher", "Winky", "Hokey"], answer: 0, level: 1, theme: "Harry Potter" },
        { question: "Qual é o feitiço usado para desarmar o oponente?", options: ["Expelliarmus", "Stupefy", "Lumos", "Avada Kedavra"], answer: 0, level: 1, theme: "Harry Potter" },
        { question: "Qual é o nome da casa onde fica o chapéu seletor?", options: ["Grifinória", "Sonserina", "Corvinal", "Lufa-Lufa"], answer: 0, level: 1, theme: "Harry Potter" },
        { question: "Qual é a cor do trem Expresso de Hogwarts?", options: ["Azul", "Verde", "Vermelho", "Preto"], answer: 2, level: 1, theme: "Harry Potter" },
        { question: "Quem era o melhor amigo de Harry Potter além de Rony?", options: ["Neville", "Draco", "Hermione", "Luna"], answer: 2, level: 1, theme: "Harry Potter" },
        { question: "O que Harry é considerado no mundo bruxo?", options: ["Auror", "Escolhido", "Príncipe Mestiço", "Herdeiro de Sonserina"], answer: 1, level: 1, theme: "Harry Potter" },
        { question: "Qual é o nome do mapa que mostra as localizações de Hogwarts?", options: ["Mapa do Maroto", "Mapa de Hogwarts", "Mapa de Hogsmeade", "Mapa do Castelo"], answer: 0, level: 1, theme: "Harry Potter" },
        { question: "Quem matou Dumbledore?", options: ["Draco Malfoy", "Voldemort", "Severus Snape", "Bellatrix Lestrange"], answer: 2, level: 1, theme: "Harry Potter" },

        // Naruto
        { question: "Qual é o sonho de Naruto?", options: ["Ser Hokage", "Ser o ninja mais forte", "Trazer Sasuke de volta", "Derrotar Akatsuki"], answer: 0, level: 1, theme: "Naruto" },
        { question: "Quem é o sensei do Time 7?", options: ["Asuma", "Jiraiya", "Kakashi", "Iruka"], answer: 2, level: 1, theme: "Naruto" },
        { question: "Qual é a habilidade especial do clã Uchiha?", options: ["Byakugan", "Sharingan", "Rinnegan", "Mangekyou"], answer: 1, level: 1, theme: "Naruto" },
        { question: "Quem é o rival de Naruto?", options: ["Gaara", "Sasuke", "Neji", "Rock Lee"], answer: 1, level: 1, theme: "Naruto" },
        { question: "Qual é o nome da raposa dentro de Naruto?", options: ["Kurama", "Shukaku", "Gyuki", "Matatabi"], answer: 0, level: 1, theme: "Naruto" },
        { question: "Qual é o título do líder da Aldeia da Folha?", options: ["Kage", "Hokage", "Raikage", "Kazekage"], answer: 1, level: 1, theme: "Naruto" },
        { question: "Quem ensinou o Rasengan a Naruto?", options: ["Jiraiya", "Kakashi", "Minato", "Tsunade"], answer: 0, level: 1, theme: "Naruto" },
        { question: "O que significa 'Akatsuki'?", options: ["Luar Vermelho", "Amanhecer", "Chuva Escura", "Vento Noturno"], answer: 1, level: 1, theme: "Naruto" },
        { question: "Qual é o nome do exame ninja em Naruto?", options: ["Chunin", "Genin", "Jonin", "ANBU"], answer: 0, level: 1, theme: "Naruto" },
        { question: "Quem é a reencarnação de Indra Otsutsuki?", options: ["Naruto", "Sasuke", "Madara", "Itachi"], answer: 1, level: 1, theme: "Naruto" },

        { question: "Quem foi o primeiro imperador do Brasil?", options: ["Dom Pedro II", "Dom Pedro I", "Joaquim José", "Tiradentes"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Em que ano ocorreu a Proclamação da República no Brasil?", options: ["1888", "1889", "1890", "1900"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Qual foi a principal atividade econômica no ciclo do ouro?", options: ["Café", "Mineração", "Pecuária", "Algodão"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Qual país colonizou o Brasil?", options: ["Espanha", "França", "Portugal", "Inglaterra"], answer: 2, level: 1, theme: "História do Brasil" },
        { question: "Quem foi o líder da Inconfidência Mineira?", options: ["Tiradentes", "Dom Pedro I", "José Bonifácio", "Dom João VI"], answer: 0, level: 1, theme: "História do Brasil" },
        { question: "Qual foi a principal causa da Guerra dos Farrapos?", options: ["Disputas territoriais", "Impostos altos", "Escravidão", "Independência"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Qual foi a capital do Brasil antes de Brasília?", options: ["São Paulo", "Rio de Janeiro", "Salvador", "Recife"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Em que ano o Brasil conquistou sua independência?", options: ["1808", "1822", "1831", "1889"], answer: 1, level: 1, theme: "História do Brasil" },
        { question: "Quem foi conhecido como o 'Patriarca da Independência'?", options: ["Tiradentes", "Dom Pedro I", "José Bonifácio", "Marechal Deodoro"], answer: 2, level: 1, theme: "História do Brasil" },
        { question: "Qual era o sistema econômico vigente no Brasil colônia?", options: ["Feudalismo", "Mercantilismo", "Capitalismo", "Socialismo"], answer: 1, level: 1, theme: "História do Brasil" },

        // Geografia
        { question: "Qual é o maior estado do Brasil em extensão territorial?", options: ["São Paulo", "Mato Grosso", "Amazonas", "Pará"], answer: 2, level: 1, theme: "Geografia" },
        { question: "Qual é o rio mais extenso do mundo?", options: ["Nilo", "Amazonas", "Yangtzé", "Mississippi"], answer: 1, level: 1, theme: "Geografia" },
        { question: "Qual é a capital da Austrália?", options: ["Sydney", "Melbourne", "Canberra", "Brisbane"], answer: 2, level: 1, theme: "Geografia" },
        { question: "Qual país tem o maior número de habitantes?", options: ["Índia", "China", "Estados Unidos", "Indonésia"], answer: 1, level: 1, theme: "Geografia" },
        { question: "Qual é a maior floresta tropical do mundo?", options: ["Floresta Amazônica", "Floresta do Congo", "Floresta Boreal", "Floresta Negra"], answer: 0, level: 1, theme: "Geografia" },
        { question: "Quantos continentes existem no mundo?", options: ["5", "6", "7", "8"], answer: 2, level: 1, theme: "Geografia" },
        { question: "Qual é o maior deserto do mundo?", options: ["Saara", "Gobi", "Kalahari", "Antártico"], answer: 3, level: 1, theme: "Geografia" },
        { question: "Qual é a capital do Brasil?", options: ["Rio de Janeiro", "São Paulo", "Brasília", "Salvador"], answer: 2, level: 1, theme: "Geografia" },
        { question: "Qual é a menor unidade federativa do Brasil?", options: ["Sergipe", "Alagoas", "Espírito Santo", "Rio de Janeiro"], answer: 0, level: 1, theme: "Geografia" },
        { question: "Qual é o oceano que banha a costa leste do Brasil?", options: ["Pacífico", "Ártico", "Índico", "Atlântico"], answer: 3, level: 1, theme: "Geografia" },

        // Atualidades
        { question: "Quem venceu as eleições presidenciais do Brasil em 2022?", options: ["Jair Bolsonaro", "Luiz Inácio Lula da Silva", "Ciro Gomes", "Simone Tebet"], answer: 1, level: 1, theme: "Atualidades" },
        { question: "Em que país ocorreu a Copa do Mundo de 2022?", options: ["Brasil", "Qatar", "Rússia", "Alemanha"], answer: 1, level: 1, theme: "Atualidades" },
        { question: "Qual é o nome da vacina contra COVID-19 desenvolvida pela Pfizer?", options: ["Coronavac", "Sputnik V", "Comirnaty", "Moderna"], answer: 2, level: 1, theme: "Atualidades" },
        { question: "Quem foi o fundador da SpaceX?", options: ["Bill Gates", "Elon Musk", "Jeff Bezos", "Mark Zuckerberg"], answer: 1, level: 1, theme: "Atualidades" },
        { question: "Qual evento climático extremo ocorreu no Sul do Brasil em junho de 2023?", options: ["Neve", "Ciclone", "Tornado", "Enchente"], answer: 1, level: 1, theme: "Atualidades" },
        { question: "Qual tecnologia foi amplamente utilizada para ensino durante a pandemia?", options: ["Impressora 3D", "Realidade Virtual", "Videoconferência", "Blockchain"], answer: 2, level: 1, theme: "Atualidades" },
        { question: "Qual é o nome do CEO atual da Apple (2025)?", options: ["Steve Jobs", "Tim Cook", "Elon Musk", "Sundar Pichai"], answer: 1, level: 1, theme: "Atualidades" },
        { question: "Qual país lidera a produção mundial de carros elétricos?", options: ["Estados Unidos", "Alemanha", "China", "Japão"], answer: 2, level: 1, theme: "Atualidades" },
        { question: "Qual país sediou os Jogos Olímpicos de 2021?", options: ["Japão", "China", "França", "Brasil"], answer: 0, level: 1, theme: "Atualidades" },
        { question: "Qual é o nome da moeda digital oficial do Brasil lançada em 2024?", options: ["Real Digital", "BitReal", "DigiReal", "RealCoin"], answer: 0, level: 1, theme: "Atualidades" },

        // Filmes da Disney
        { question: "Qual é o nome da protagonista de 'Frozen'?", options: ["Elsa", "Anna", "Rapunzel", "Moana"], answer: 0, level: 1, theme: "Filmes da Disney" },
        { question: "Qual animal é Simba em 'O Rei Leão'?", options: ["Leão", "Tigre", "Cachorro", "Urso"], answer: 0, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome do cowboy em 'Toy Story'?", options: ["Buzz", "Woody", "Andy", "Jessie"], answer: 1, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome do ratinho chefe em 'Ratatouille'?", options: ["Remy", "Emile", "Django", "Gusteau"], answer: 0, level: 1, theme: "Filmes da Disney" },
        { question: "Qual princesa Disney tem um dragão como amigo?", options: ["Jasmine", "Mulan", "Ariel", "Cinderela"], answer: 1, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome do robô em 'Wall-E'?", options: ["EVA", "WALL-E", "MO", "R2-D2"], answer: 1, level: 1, theme: "Filmes da Disney" },
        { question: "Qual princesa Disney adormece após espetar o dedo?", options: ["Aurora", "Branca de Neve", "Cinderela", "Elsa"], answer: 0, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome do vilão em 'Aladdin'?", options: ["Jafar", "Scar", "Hades", "Ursula"], answer: 0, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome da cidade em 'Zootopia'?", options: ["Zootropolis", "Zootopia", "Animália", "Animal City"], answer: 1, level: 1, theme: "Filmes da Disney" },
        { question: "Qual é o nome da garota em 'Valente'?", options: ["Elsa", "Merida", "Anna", "Moana"], answer: 1, level: 1, theme: "Filmes da Disney" }

    ],
    
    prizes: [
        {actions: ["beijo", "cheiro", "massagem", "lambida", "mordida", "carícia"]},
        {locations: ["boca", "bochecha", "orelha", "nariz", "testa", "nuca", "pescoço", "peito", "costas", "costela", "virilha", "coxas", "panturrilha", "pés", "mãos"]},
        {times: ["5", "10", "15", "20"]}
    ],

    currentPlayerIndex: 0,
    currentQuestionIndex: null
};



// Referências dos elementos DOM
const questionElement = document.getElementById("question");
const optionsElement = document.getElementById("options");
const resultElement = document.getElementById("result");
const prizeElement = document.getElementById("prize");
const scoreElements = [
    document.getElementById("score-jogador1"),
    document.getElementById("score-jogador2")
];
const totensElements = [
    document.getElementById("totens-jogador1"),
    document.getElementById("totens-jogador2")
];
const checkButton = document.getElementById("check-btn");
const nextButton = document.getElementById("next-btn");

// Função para embaralhar as perguntas
function shuffleQuestions() {
    gameData.questions = gameData.questions.sort(() => Math.random() - 0.5);
}

// Atualiza pontuação e totens no DOM
function updateScores() {
    gameData.players.forEach((player, index) => {
        scoreElements[index].innerText = player.score;
        totensElements[index].innerHTML = "";
        for (let i = 0; i < player.totens; i++) {
            totensElements[index].innerHTML += "<i class='totem-icon'>🟢</i>";
        }
    });
}

// Exibe a pergunta atual
function displayQuestion() {
    const question = gameData.questions[gameData.currentQuestionIndex];
    questionElement.innerText = question.question;
    optionsElement.innerHTML = "";
    question.options.forEach((option, index) => {
        optionsElement.innerHTML += `
            <div class="form-check">
                <input class="form-check-input" type="radio" name="option" id="option${index}" value="${index}">
                <label class="form-check-label" for="option${index}">${option}</label>
            </div>
        `;
    });
    resultElement.innerText = "Selecione uma resposta";
   // prizeElement.innerText = "Prêmio: Nenhum";

    // Destaque para o jogador atual
    document.getElementById("jogador1-name").style.fontWeight = gameData.currentPlayerIndex === 0 ? "bold" : "normal";
    document.getElementById("jogador1-name").style.fontSize = gameData.currentPlayerIndex === 0 ? "18px" : "16px";
    document.getElementById("jogador2-name").style.fontWeight = gameData.currentPlayerIndex === 1 ? "bold" : "normal";
    document.getElementById("jogador2-name").style.fontSize = gameData.currentPlayerIndex === 1 ? "18px" : "16px";
}

function getRandomPrize(gameData) {
    const { prizes } = gameData;

    // Obtém aleatoriamente um elemento de cada categoria
    const action = prizes[0].actions[Math.floor(Math.random() * prizes[0].actions.length)];
    const location = prizes[1].locations[Math.floor(Math.random() * prizes[1].locations.length)];
    const time = prizes[2].times[Math.floor(Math.random() * prizes[2].times.length)];

    return `${action} na(o) ${location} por ${time} segundos`;
}

// Verifica a resposta do jogador
function checkAnswer() {
    const question = gameData.questions[gameData.currentQuestionIndex];
    const selectedOption = document.querySelector('input[name="option"]:checked');
    if (!selectedOption) {
        alert("Por favor, selecione uma opção.");
        return;
    }

    const isCorrect = parseInt(selectedOption.value) === question.answer;
    const currentPlayer = gameData.players[gameData.currentPlayerIndex];
    const opponentPlayer = gameData.players[1 - gameData.currentPlayerIndex];

    if (isCorrect) {
        resultElement.innerText = "Correto!";
        opponentPlayer.score--;
        //const prizeLevel = 4 - currentPlayer.totens; // Baseado nos totens do jogador atual
        prizeElement.innerText = getRandomPrize(gameData);
        
        //Precisa implementar a lógica de nivel de prêmio
        //const prize = gameData.prizes.find(p => p.level === prizeLevel);
        // if (prize) {
        //     prizeElement.innerText = `Prêmio: ${prize.action} por ${prize.time} no(a) ${prize.location}`;
        // } else {
        //     prizeElement.innerText = "Prêmio: Nenhum prêmio disponível para este nível.";
        // }
    } else {
        resultElement.innerText = "Errado!";
        currentPlayer.score--;
    }

    // Atualizar totens se necessário
    if (opponentPlayer.score % 3 === 0 && opponentPlayer.score >= 0) {
        opponentPlayer.totens--;
        currentPlayer.totens++;
    }

    updateScores();
    checkGameOver();
    checkButton.disabled = true;
    nextButton.disabled = false;
}

// Verifica se o jogo acabou
function checkGameOver() {
    gameData.players.forEach((player, index) => {
        if (player.totens === 0) {
            $("#gameOverModal").modal("show");
            document.getElementById("game-over-message").innerText = `${gameData.players[1 - index].name} venceu!`;
        }
    });
}

// Prepara a próxima rodada
function nextRound() {
    gameData.currentPlayerIndex = 1 - gameData.currentPlayerIndex;
    gameData.currentQuestionIndex = (gameData.currentQuestionIndex + 1) % gameData.questions.length;
    displayQuestion();
    checkButton.disabled = false;
    nextButton.disabled = true;

    prizeElement.innerText = ""

}

// Reinicia o jogo
function restartGame() {
    gameData.players.forEach(player => {
        player.score = 12;
        player.totens = 4;
    });
    shuffleQuestions();
    gameData.currentQuestionIndex = 0;
    gameData.currentPlayerIndex = 0;
    updateScores();
    displayQuestion();
    $("#gameOverModal").modal("hide");
}

// Inicializa o jogo
document.addEventListener("DOMContentLoaded", () => {
    shuffleQuestions();
    gameData.currentQuestionIndex = 0;
    updateScores();
    displayQuestion();

    checkButton.addEventListener("click", checkAnswer);
    nextButton.addEventListener("click", nextRound);
});