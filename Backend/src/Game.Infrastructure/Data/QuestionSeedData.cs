using Game.Domain.Entities;

namespace Game.Infrastructure.Data;

public static class QuestionSeedData
{
    public static List<Question> GetQuestions()
    {
        var legacyRaw = new (string Text, string[] Options, int Answer, int Level, string Theme)[]
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

        var expansionRaw = new (string Text, string[] Options, int Answer, int Level, string Theme)[]
        {
            // Biologia — nível 1
            ("Qual é a unidade estrutural e funcional básica dos seres vivos?", new[] {"Célula", "Tecido", "Órgão", "Molécula"}, 0, 1, "Biologia"),
            ("Qual órgão é responsável pelas trocas gasosas na maioria dos mamíferos?", new[] {"Fígado", "Rins", "Pulmões", "Estômago"}, 2, 1, "Biologia"),
            ("Qual gás as plantas absorvem do ambiente para realizar a fotossíntese?", new[] {"Oxigênio", "Dióxido de carbono", "Nitrogênio", "Hidrogênio"}, 1, 1, "Biologia"),
            ("Qual órgão humano impulsiona o sangue pelo sistema circulatório?", new[] {"Baço", "Pâncreas", "Fígado", "Coração"}, 3, 1, "Biologia"),
            ("Qual molécula armazena a maior parte da informação genética dos organismos celulares?", new[] {"ATP", "DNA", "Glicose", "Colágeno"}, 1, 1, "Biologia"),

            // Biologia — nível 2
            ("Em células eucarióticas, qual organela produz a maior parte do ATP durante a respiração aeróbia?", new[] {"Lisossomo", "Complexo golgiense", "Retículo endoplasmático", "Mitocôndria"}, 3, 2, "Biologia"),
            ("Como se chama a relação ecológica em que as duas espécies participantes obtêm benefício?", new[] {"Mutualismo", "Predação", "Amensalismo", "Parasitismo"}, 0, 2, "Biologia"),
            ("Quais células do sistema imune podem se diferenciar em plasmócitos produtores de anticorpos?", new[] {"Neutrófilos", "Hemácias", "Linfócitos B", "Plaquetas"}, 2, 2, "Biologia"),
            ("Qual tecido vascular conduz principalmente água e sais minerais das raízes para as partes aéreas da planta?", new[] {"Floema", "Epiderme", "Xilema", "Câmbio"}, 2, 2, "Biologia"),
            ("Em um organismo diploide, o que caracteriza um par de cromossomos homólogos?", new[] {"Um cromossomo de origem materna e outro de origem paterna com os mesmos loci gênicos", "Duas cópias idênticas formadas na duplicação do DNA", "Dois cromossomos que carregam necessariamente alelos iguais", "Um cromossomo autossômico e um cromossomo sexual"}, 0, 2, "Biologia"),

            // Biologia — nível 3
            ("Qual conjunto de condições é compatível com o equilíbrio de Hardy-Weinberg?", new[] {"População pequena, seleção natural e migração intensa", "Acasalamento preferencial, mutação frequente e ausência de deriva", "Seleção estabilizadora, fluxo gênico e população constante", "População grande, acasalamento aleatório e ausência de seleção, mutação e migração"}, 3, 3, "Biologia"),
            ("Em qual estrutura do néfron ocorre a filtração inicial do plasma sanguíneo?", new[] {"Alça de Henle", "Glomérulo", "Túbulo contorcido distal", "Ducto coletor"}, 1, 3, "Biologia"),
            ("Na conjugação bacteriana, como o DNA é geralmente transferido entre as células?", new[] {"Por vírus que infectam as duas bactérias", "Pela captação de DNA livre no ambiente", "Por divisão binária sincronizada", "Por contato direto mediado por uma estrutura de conjugação"}, 3, 3, "Biologia"),
            ("Nas plantas com metabolismo CAM, em que período os estômatos normalmente se abrem para captar dióxido de carbono?", new[] {"Ao meio-dia", "Durante a noite", "Somente ao amanhecer", "Durante toda a fase clara"}, 1, 3, "Biologia"),
            ("Qual processo inicia mais diretamente uma especiação alopátrica?", new[] {"Duplicação do DNA em todas as células", "Acasalamento aleatório em uma única população", "Isolamento geográfico entre populações", "Aumento uniforme da taxa de mutação"}, 2, 3, "Biologia"),

            // Biologia — nível 4
            ("Qual atividade enzimática permite à telomerase estender as extremidades dos cromossomos?", new[] {"Transcriptase reversa dependente de RNA", "DNA ligase dependente de ATP", "Endonuclease de restrição", "RNA polimerase dependente de DNA"}, 0, 4, "Biologia"),
            ("Qual evento é característico da via intrínseca da apoptose em células animais?", new[] {"Liberação de citocromo c pela mitocôndria", "Ativação direta de receptores de morte por ligantes extracelulares", "Ruptura osmótica imediata da membrana plasmática", "Duplicação dos centrossomos antes da mitose"}, 0, 4, "Biologia"),
            ("Na fixação inicial de carbono das plantas C4, qual combinação está correta?", new[] {"RuBP e rubisco formam gliceraldeído-3-fosfato", "Piruvato e ATP formam diretamente glicose", "Malato e NADPH formam fosfoenolpiruvato", "Fosfoenolpiruvato e PEP carboxilase formam oxaloacetato"}, 3, 4, "Biologia"),
            ("Que tipo de antígeno é normalmente apresentado por moléculas de MHC classe I a linfócitos T CD8+?", new[] {"Polissacarídeos extracelulares intactos", "Peptídeos derivados de proteínas produzidas no interior da célula", "Lipídios capturados exclusivamente por fagocitose", "Proteínas extracelulares completas sem processamento"}, 1, 4, "Biologia"),
            ("Em Escherichia coli, qual é o estado esperado do operon lac quando há muita glicose e não há lactose?", new[] {"Transcrição máxima pela ligação do complexo CAP-cAMP", "Transcrição constitutiva pela inativação do repressor", "Transcrição mínima, com o repressor ligado e baixa ativação por CAP", "Ativação exclusiva do gene lacZ e repressão de lacY"}, 2, 4, "Biologia"),

            // Artes — nível 1
            ("Quem pintou a obra conhecida como Mona Lisa?", new[] {"Michelangelo", "Rafael", "Leonardo da Vinci", "Sandro Botticelli"}, 2, 1, "Artes"),
            ("Na mistura tradicional de pigmentos, qual cor resulta da combinação de azul e amarelo?", new[] {"Laranja", "Verde", "Violeta", "Marrom"}, 1, 1, "Artes"),
            ("Qual característica distingue a escultura da pintura de modo mais geral?", new[] {"Uso obrigatório de mármore", "Ausência de formas humanas", "Produção exclusiva em espaços públicos", "Ocupação tridimensional do espaço"}, 3, 1, "Artes"),
            ("Qual artista brasileira pintou Abaporu?", new[] {"Tarsila do Amaral", "Anita Malfatti", "Djanira da Motta e Silva", "Lygia Clark"}, 0, 1, "Artes"),
            ("Qual arquiteto brasileiro projetou edifícios marcantes de Brasília, como o Congresso Nacional?", new[] {"Lúcio Costa", "Affonso Eduardo Reidy", "Lina Bo Bardi", "Oscar Niemeyer"}, 3, 1, "Artes"),

            // Artes — nível 2
            ("Que preocupação foi central para muitos pintores impressionistas?", new[] {"Registrar efeitos momentâneos de luz e cor", "Representar apenas narrativas mitológicas", "Eliminar totalmente as pinceladas visíveis", "Imitar com precisão a escultura clássica"}, 0, 2, "Artes"),
            ("Na técnica do afresco verdadeiro, onde os pigmentos diluídos em água são aplicados?", new[] {"Sobre tela preparada com óleo", "Sobre madeira coberta por cera", "Sobre reboco de cal ainda úmido", "Sobre papel seco envernizado"}, 2, 2, "Artes"),
            ("Qual artista pintou Guernica como resposta ao bombardeio da cidade basca?", new[] {"Joan Miró", "Pablo Picasso", "Salvador Dalí", "Diego Rivera"}, 1, 2, "Artes"),
            ("A tradição artística ukiyo-e se desenvolveu principalmente em qual país?", new[] {"China", "Japão", "Coreia", "Índia"}, 1, 2, "Artes"),
            ("Antônio Francisco Lisboa, o Aleijadinho, é associado principalmente a qual contexto artístico?", new[] {"Modernismo paulista", "Neoclassicismo francês", "Barroco colonial brasileiro", "Muralismo mexicano"}, 2, 2, "Artes"),

            // Artes — nível 3
            ("O que define a técnica pictórica conhecida como chiaroscuro?", new[] {"Contraste acentuado entre luz e sombra para modelar volumes", "Aplicação de pontos de cores puras sem mistura", "Uso exclusivo de linhas geométricas sem gradação", "Pintura feita apenas com pigmentos monocromáticos"}, 0, 3, "Artes"),
            ("Quem fundou a escola Bauhaus em Weimar, em 1919?", new[] {"Ludwig Mies van der Rohe", "Paul Klee", "Wassily Kandinsky", "Walter Gropius"}, 3, 3, "Artes"),
            ("Os chamados Bronzes de Benin foram produzidos para a corte de qual reino histórico africano?", new[] {"Reino de Benin", "Império do Mali", "Reino do Congo", "Império Axumita"}, 0, 3, "Artes"),
            ("Qual artista é associado ao Monumento à Terceira Internacional, projeto emblemático do construtivismo russo?", new[] {"Kazimir Malevich", "El Lissitzky", "Aleksandr Rodchenko", "Vladimir Tatlin"}, 3, 3, "Artes"),
            ("Quem criou a gravura A Grande Onda de Kanagawa?", new[] {"Utagawa Hiroshige", "Katsushika Hokusai", "Kitagawa Utamaro", "Tōshūsai Sharaku"}, 1, 3, "Artes"),

            // Artes — nível 4
            ("Na pintura encáustica, qual material funciona tradicionalmente como aglutinante dos pigmentos?", new[] {"Gema de ovo", "Óleo de linhaça", "Cera aquecida", "Goma arábica"}, 2, 4, "Artes"),
            ("Na escultura clássica, o termo contrapposto descreve qual organização corporal?", new[] {"Simetria rígida com o peso igualmente distribuído", "Torção completa do tronco sem apoio das pernas", "Peso apoiado principalmente em uma perna, com compensação do restante do corpo", "Figura ajoelhada com os braços paralelos"}, 2, 4, "Artes"),
            ("Que efeito Leonardo da Vinci buscava com a técnica do sfumato?", new[] {"Transições suaves de tom e contornos pouco definidos", "Relevo espesso produzido com grande quantidade de tinta", "Contraste de cores complementares em áreas planas", "Linhas incisivas gravadas sobre a camada de tinta"}, 0, 4, "Artes"),
            ("Na arquitetura grega clássica, qual ordem se reconhece pelas volutas em espiral de seu capitel?", new[] {"Dórica", "Toscana", "Coríntia", "Jônica"}, 3, 4, "Artes"),
            ("Qual artista participou do neoconcretismo brasileiro e criou a série Bichos?", new[] {"Tomie Ohtake", "Lygia Clark", "Djanira da Motta e Silva", "Georgina de Albuquerque"}, 1, 4, "Artes"),

            // Cinema — nível 1
            ("Quem dirigiu o filme E.T. — O Extraterrestre?", new[] {"George Lucas", "Steven Spielberg", "Robert Zemeckis", "James Cameron"}, 1, 1, "Cinema"),
            ("Em O Mágico de Oz, para qual lugar Dorothy deseja voltar?", new[] {"Califórnia", "Nova York", "Texas", "Kansas"}, 3, 1, "Cinema"),
            ("Em qual cidade brasileira se passa a maior parte de Cidade de Deus?", new[] {"Rio de Janeiro", "Salvador", "Recife", "São Paulo"}, 0, 1, "Cinema"),
            ("Como funciona a técnica de animação stop motion?", new[] {"Atores são filmados continuamente em câmera lenta", "Desenhos são gerados apenas por inteligência artificial", "Objetos são fotografados quadro a quadro com pequenas mudanças de posição", "Duas cenas são projetadas ao mesmo tempo na mesma tela"}, 2, 1, "Cinema"),
            ("A qual saga cinematográfica pertence o personagem Darth Vader?", new[] {"Star Wars", "Star Trek", "Matrix", "Alien"}, 0, 1, "Cinema"),

            // Cinema — nível 2
            ("Quem dirigiu o filme Psicose, lançado em 1960?", new[] {"Billy Wilder", "Orson Welles", "Alfred Hitchcock", "John Ford"}, 2, 2, "Cinema"),
            ("Em Central do Brasil, qual é o nome da personagem que escreve cartas para pessoas analfabetas?", new[] {"Irene", "Ana", "Isadora", "Dora"}, 3, 2, "Cinema"),
            ("O que é som diegético em um filme?", new[] {"Som acrescentado apenas ao trailer", "Som cuja fonte pertence ao universo narrativo da cena", "Música executada obrigatoriamente por uma orquestra", "Ruído removido durante a pós-produção"}, 1, 2, "Cinema"),
            ("Quem dirigiu o clássico japonês Os Sete Samurais?", new[] {"Yasujirō Ozu", "Kenji Mizoguchi", "Hiroshi Teshigahara", "Akira Kurosawa"}, 3, 2, "Cinema"),
            ("Na cerimônia do Oscar de 2020, qual filme se tornou o primeiro em língua não inglesa a vencer a categoria de melhor filme?", new[] {"Roma", "Parasita", "Amor", "O Tigre e o Dragão"}, 1, 2, "Cinema"),

            // Cinema — nível 3
            ("Qual filme de Jean-Luc Godard tornou célebre o uso de jump cuts na Nouvelle Vague francesa?", new[] {"Os Incompreendidos", "Hiroshima, Meu Amor", "Acossado", "Cléo das 5 às 7"}, 2, 3, "Cinema"),
            ("A frase 'uma câmera na mão e uma ideia na cabeça' é tradicionalmente associada a qual cineasta do Cinema Novo?", new[] {"Glauber Rocha", "Nelson Pereira dos Santos", "Joaquim Pedro de Andrade", "Leon Hirszman"}, 0, 3, "Cinema"),
            ("Qual é a finalidade principal da regra dos 180 graus na filmagem de um diálogo?", new[] {"Fixar a duração máxima de cada plano", "Determinar a abertura do diafragma", "Manter coerentes as posições e direções dos personagens na tela", "Impedir qualquer movimento de câmera"}, 2, 3, "Cinema"),
            ("Qual combinação é característica do neorrealismo italiano do pós-guerra?", new[] {"Filmagens em locações e presença de atores não profissionais", "Cenários futuristas e efeitos ópticos abstratos", "Musicais em estúdio com coreografias grandiosas", "Narrativas mitológicas com estrelas internacionais"}, 0, 3, "Cinema"),
            ("Qual recurso narrativo de Rashomon se tornou referência na história do cinema?", new[] {"Uma história contada inteiramente de trás para frente", "Relatos conflitantes do mesmo acontecimento", "Ausência completa de personagens humanos", "Alternância entre animação e documentário"}, 1, 3, "Cinema"),

            // Cinema — nível 4
            ("O que demonstra o chamado efeito Kuleshov?", new[] {"A lente grande-angular sempre aumenta a velocidade aparente", "O som síncrono torna desnecessária a montagem", "A iluminação frontal elimina a percepção de profundidade", "O sentido atribuído a um plano muda conforme os planos que o acompanham"}, 3, 4, "Cinema"),
            ("O que uma lente anamórfica faz durante a captação em película para permitir uma imagem panorâmica?", new[] {"Recorta automaticamente as bordas superior e inferior", "Duplica cada quadro em duas exposições", "Gira o quadro em noventa graus", "Comprime horizontalmente a imagem, que depois é descomprimida na projeção"}, 3, 4, "Cinema"),
            ("Quais cineastas lançaram conjuntamente o manifesto Dogma 95?", new[] {"Ingmar Bergman e Carl Theodor Dreyer", "Agnès Varda e Jacques Demy", "Lars von Trier e Thomas Vinterberg", "Werner Herzog e Wim Wenders"}, 2, 4, "Cinema"),
            ("Qual longa-metragem de 1927 ficou conhecido por popularizar sequências de diálogo sincronizado no cinema comercial?", new[] {"O Cantor de Jazz", "Metrópolis", "Aurora", "Napoleão"}, 0, 4, "Cinema"),
            ("Quem dirigiu o documentário experimental Um Homem com uma Câmera, de 1929?", new[] {"Sergei Eisenstein", "Dziga Vertov", "Vsevolod Pudovkin", "Aleksandr Dovjenko"}, 1, 4, "Cinema"),
        };

        return legacyRaw.Concat(expansionRaw).Select((q, index) => new Question
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
