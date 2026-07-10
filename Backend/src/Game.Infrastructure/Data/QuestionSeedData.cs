using Game.Domain.Entities;

namespace Game.Infrastructure.Data;

public static class QuestionSeedData
{
    public static List<Question> GetQuestions()
    {
        var raw = new (string Text, string[] Options, int Answer, int Level, string Theme)[]
        {
            // Harry Potter
            ("Qual é o nome da escola de magia frequentada por Harry Potter?", new[] {"Durmstrang", "Hogwarts", "Beauxbatons", "Ilvermorny"}, 1, 1, "Harry Potter"),
            ("Quem é o diretor de Hogwarts na maior parte da série?", new[] {"Severus Snape", "Minerva McGonagall", "Alvo Dumbledore", "Hagrid"}, 2, 1, "Harry Potter"),
            ("Qual é o nome do elfo doméstico de Harry Potter?", new[] {"Dobby", "Kreacher", "Winky", "Hokey"}, 0, 1, "Harry Potter"),
            ("Qual é o feitiço usado para desarmar o oponente?", new[] {"Expelliarmus", "Stupefy", "Lumos", "Avada Kedavra"}, 0, 1, "Harry Potter"),
            ("Qual é o nome da casa onde fica o chapéu seletor?", new[] {"Grifinória", "Sonserina", "Corvinal", "Lufa-Lufa"}, 0, 1, "Harry Potter"),
            ("Qual é a cor do trem Expresso de Hogwarts?", new[] {"Azul", "Verde", "Vermelho", "Preto"}, 2, 1, "Harry Potter"),
            ("Quem era o melhor amigo de Harry Potter além de Rony?", new[] {"Neville", "Draco", "Hermione", "Luna"}, 2, 1, "Harry Potter"),
            ("O que Harry é considerado no mundo bruxo?", new[] {"Auror", "Escolhido", "Príncipe Mestiço", "Herdeiro de Sonserina"}, 1, 1, "Harry Potter"),
            ("Qual é o nome do mapa que mostra as localizações de Hogwarts?", new[] {"Mapa do Maroto", "Mapa de Hogwarts", "Mapa de Hogsmeade", "Mapa do Castelo"}, 0, 1, "Harry Potter"),
            ("Quem matou Dumbledore?", new[] {"Draco Malfoy", "Voldemort", "Severus Snape", "Bellatrix Lestrange"}, 2, 1, "Harry Potter"),

            // Naruto
            ("Qual é o sonho de Naruto?", new[] {"Ser Hokage", "Ser o ninja mais forte", "Trazer Sasuke de volta", "Derrotar Akatsuki"}, 0, 1, "Naruto"),
            ("Quem é o sensei do Time 7?", new[] {"Asuma", "Jiraiya", "Kakashi", "Iruka"}, 2, 1, "Naruto"),
            ("Qual é a habilidade especial do clã Uchiha?", new[] {"Byakugan", "Sharingan", "Rinnegan", "Mangekyou"}, 1, 1, "Naruto"),
            ("Quem é o rival de Naruto?", new[] {"Gaara", "Sasuke", "Neji", "Rock Lee"}, 1, 1, "Naruto"),
            ("Qual é o nome da raposa dentro de Naruto?", new[] {"Kurama", "Shukaku", "Gyuki", "Matatabi"}, 0, 1, "Naruto"),
            ("Qual é o título do líder da Aldeia da Folha?", new[] {"Kage", "Hokage", "Raikage", "Kazekage"}, 1, 1, "Naruto"),
            ("Quem ensinou o Rasengan a Naruto?", new[] {"Jiraiya", "Kakashi", "Minato", "Tsunade"}, 0, 1, "Naruto"),
            ("O que significa 'Akatsuki'?", new[] {"Luar Vermelho", "Amanhecer", "Chuva Escura", "Vento Noturno"}, 1, 1, "Naruto"),
            ("Qual é o nome do exame ninja em Naruto?", new[] {"Chunin", "Genin", "Jonin", "ANBU"}, 0, 1, "Naruto"),
            ("Quem é a reencarnação de Indra Otsutsuki?", new[] {"Naruto", "Sasuke", "Madara", "Itachi"}, 1, 1, "Naruto"),

            // História do Brasil
            ("Quem foi o primeiro imperador do Brasil?", new[] {"Dom Pedro II", "Dom Pedro I", "Joaquim José", "Tiradentes"}, 1, 1, "História do Brasil"),
            ("Em que ano ocorreu a Proclamação da República no Brasil?", new[] {"1888", "1889", "1890", "1900"}, 1, 1, "História do Brasil"),
            ("Qual foi a principal atividade econômica no ciclo do ouro?", new[] {"Café", "Mineração", "Pecuária", "Algodão"}, 1, 1, "História do Brasil"),
            ("Qual país colonizou o Brasil?", new[] {"Espanha", "França", "Portugal", "Inglaterra"}, 2, 1, "História do Brasil"),
            ("Quem foi o líder da Inconfidência Mineira?", new[] {"Tiradentes", "Dom Pedro I", "José Bonifácio", "Dom João VI"}, 0, 1, "História do Brasil"),
            ("Qual foi a principal causa da Guerra dos Farrapos?", new[] {"Disputas territoriais", "Impostos altos", "Escravidão", "Independência"}, 1, 1, "História do Brasil"),
            ("Qual foi a capital do Brasil antes de Brasília?", new[] {"São Paulo", "Rio de Janeiro", "Salvador", "Recife"}, 1, 1, "História do Brasil"),
            ("Em que ano o Brasil conquistou sua independência?", new[] {"1808", "1822", "1831", "1889"}, 1, 1, "História do Brasil"),
            ("Quem foi conhecido como o 'Patriarca da Independência'?", new[] {"Tiradentes", "Dom Pedro I", "José Bonifácio", "Marechal Deodoro"}, 2, 1, "História do Brasil"),
            ("Qual era o sistema econômico vigente no Brasil colônia?", new[] {"Feudalismo", "Mercantilismo", "Capitalismo", "Socialismo"}, 1, 1, "História do Brasil"),

            // Geografia
            ("Qual é o maior estado do Brasil em extensão territorial?", new[] {"São Paulo", "Mato Grosso", "Amazonas", "Pará"}, 2, 1, "Geografia"),
            ("Qual é o rio mais extenso do mundo?", new[] {"Nilo", "Amazonas", "Yangtzé", "Mississippi"}, 1, 1, "Geografia"),
            ("Qual é a capital da Austrália?", new[] {"Sydney", "Melbourne", "Canberra", "Brisbane"}, 2, 1, "Geografia"),
            ("Qual país tem o maior número de habitantes?", new[] {"Índia", "China", "Estados Unidos", "Indonésia"}, 1, 1, "Geografia"),
            ("Qual é a maior floresta tropical do mundo?", new[] {"Floresta Amazônica", "Floresta do Congo", "Floresta Boreal", "Floresta Negra"}, 0, 1, "Geografia"),
            ("Quantos continentes existem no mundo?", new[] {"5", "6", "7", "8"}, 2, 1, "Geografia"),
            ("Qual é o maior deserto do mundo?", new[] {"Saara", "Gobi", "Kalahari", "Antártico"}, 3, 1, "Geografia"),
            ("Qual é a capital do Brasil?", new[] {"Rio de Janeiro", "São Paulo", "Brasília", "Salvador"}, 2, 1, "Geografia"),
            ("Qual é a menor unidade federativa do Brasil?", new[] {"Sergipe", "Alagoas", "Espírito Santo", "Rio de Janeiro"}, 0, 1, "Geografia"),
            ("Qual é o oceano que banha a costa leste do Brasil?", new[] {"Pacífico", "Ártico", "Índico", "Atlântico"}, 3, 1, "Geografia"),

            // Atualidades
            ("Quem venceu as eleições presidenciais do Brasil em 2022?", new[] {"Jair Bolsonaro", "Luiz Inácio Lula da Silva", "Ciro Gomes", "Simone Tebet"}, 1, 1, "Atualidades"),
            ("Em que país ocorreu a Copa do Mundo de 2022?", new[] {"Brasil", "Qatar", "Rússia", "Alemanha"}, 1, 1, "Atualidades"),
            ("Qual é o nome da vacina contra COVID-19 desenvolvida pela Pfizer?", new[] {"Coronavac", "Sputnik V", "Comirnaty", "Moderna"}, 2, 1, "Atualidades"),
            ("Quem foi o fundador da SpaceX?", new[] {"Bill Gates", "Elon Musk", "Jeff Bezos", "Mark Zuckerberg"}, 1, 1, "Atualidades"),
            ("Qual evento climático extremo ocorreu no Sul do Brasil em junho de 2023?", new[] {"Neve", "Ciclone", "Tornado", "Enchente"}, 1, 1, "Atualidades"),
            ("Qual tecnologia foi amplamente utilizada para ensino durante a pandemia?", new[] {"Impressora 3D", "Realidade Virtual", "Videoconferência", "Blockchain"}, 2, 1, "Atualidades"),
            ("Qual é o nome do CEO atual da Apple (2025)?", new[] {"Steve Jobs", "Tim Cook", "Elon Musk", "Sundar Pichai"}, 1, 1, "Atualidades"),
            ("Qual país lidera a produção mundial de carros elétricos?", new[] {"Estados Unidos", "Alemanha", "China", "Japão"}, 2, 1, "Atualidades"),
            ("Qual país sediou os Jogos Olímpicos de 2021?", new[] {"Japão", "China", "França", "Brasil"}, 0, 1, "Atualidades"),
            ("Qual é o nome da moeda digital oficial do Brasil lançada em 2024?", new[] {"Real Digital", "BitReal", "DigiReal", "RealCoin"}, 0, 1, "Atualidades"),

            // Filmes da Disney
            ("Qual é o nome da protagonista de 'Frozen'?", new[] {"Elsa", "Anna", "Rapunzel", "Moana"}, 0, 1, "Filmes da Disney"),
            ("Qual animal é Simba em 'O Rei Leão'?", new[] {"Leão", "Tigre", "Cachorro", "Urso"}, 0, 1, "Filmes da Disney"),
            ("Qual é o nome do cowboy em 'Toy Story'?", new[] {"Buzz", "Woody", "Andy", "Jessie"}, 1, 1, "Filmes da Disney"),
            ("Qual é o nome do ratinho chefe em 'Ratatouille'?", new[] {"Remy", "Emile", "Django", "Gusteau"}, 0, 1, "Filmes da Disney"),
            ("Qual princesa Disney tem um dragão como amigo?", new[] {"Jasmine", "Mulan", "Ariel", "Cinderela"}, 1, 1, "Filmes da Disney"),
            ("Qual é o nome do robô em 'Wall-E'?", new[] {"EVA", "WALL-E", "MO", "R2-D2"}, 1, 1, "Filmes da Disney"),
            ("Qual princesa Disney adormece após espetar o dedo?", new[] {"Aurora", "Branca de Neve", "Cinderela", "Elsa"}, 0, 1, "Filmes da Disney"),
            ("Qual é o nome do vilão em 'Aladdin'?", new[] {"Jafar", "Scar", "Hades", "Ursula"}, 0, 1, "Filmes da Disney"),
            ("Qual é o nome da cidade em 'Zootopia'?", new[] {"Zootropolis", "Zootopia", "Animália", "Animal City"}, 1, 1, "Filmes da Disney"),
            ("Qual é o nome da garota em 'Valente'?", new[] {"Elsa", "Merida", "Anna", "Moana"}, 1, 1, "Filmes da Disney"),
        };

        return raw.Select((q, index) => new Question
        {
            Id = index,
            Text = q.Text,
            Options = q.Options.ToList(),
            CorrectAnswerIndex = q.Answer,
            Level = q.Level,
            Theme = q.Theme
        }).ToList();
    }
}
