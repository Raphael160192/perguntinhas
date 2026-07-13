# Proposta: nivelamento inteligente dos prêmios

> Proposta de produto e arquitetura **implementada atrás de feature flag** em
> 13/07/2026. A seleção inteligente permanece desligada por padrão até o rollout.
> O escopo deste documento é exclusivamente a coerência das combinações, o
> nivelamento progressivo, a relação com as roupas e a variedade dos prêmios.
>
> Plano de engenharia correspondente:
> [`implementação do nivelamento inteligente`](plans/implementacao-nivelamento-inteligente-premios.md).

## 1. Problema atual

O gerador legado, mantido para rollback enquanto a flag estiver desligada,
sorteia ação, local e duração de forma totalmente independente.
Isso cria 360 combinações matematicamente válidas, mas não necessariamente
coerentes ou agradáveis. “Mordida no nariz” é um bom exemplo: os três componentes
existem no catálogo, mas sua combinação não serve ao objetivo de construir tensão
com descontração, leveza e prazer.

Há três problemas estruturais:

1. **Compatibilidade:** nem toda ação combina com todo local ou duração.
2. **Ritmo:** um prêmio intenso pode aparecer na primeira rodada e um prêmio muito
   leve pode aparecer perto do final.
3. **Contexto:** roupas, papéis de quem oferece/recebe e histórico recente não
   participam do sorteio.

## 2. Escopo desta proposta

### 2.1 Incluído

- catálogo de prêmios completos e previamente curados;
- quatro níveis estáticos de intensidade;
- progressão calculada a partir das peças perdidas;
- transição gradual entre o nível anterior e o atual;
- compatibilidade entre ação, local e forma de execução;
- regras de acessibilidade conforme o estado das roupas;
- papéis claros para quem oferece e quem recebe;
- pesos, variedade e cooldown contra repetições;
- persistência e observabilidade técnica do sorteio.

### 2.2 Fora do escopo

Os seguintes assuntos serão tratados no épico específico de configurações de
partida preparado pela PO e não fazem parte desta proposta:

- coleta de consentimento;
- preferências, limites ou bloqueios configurados pelos jogadores;
- votação ou confirmação do casal para aumentar intensidade;
- formulários antes ou durante a partida;
- aprendizado baseado em “Feito”, “Pular” ou feedback dos jogadores;
- idade, elegibilidade, privacidade e obrigações legais ou jurídicas;
- políticas e textos regulatórios da experiência.

O nivelamento descrito aqui não depende dessas features. Quando o épico futuro
for implementado, suas regras poderão atuar como filtros adicionais antes do
sorteio, sem alterar a estrutura central de templates e níveis.

## 3. Mudança central: de componentes livres para templates curados

O gerador não deve mais executar:

```text
ação aleatória × local aleatório × duração aleatória
```

Ele deve escolher entre **templates de prêmio previamente curados**. Cada template
é uma unidade completa de conteúdo e declara quais parâmetros podem variar.

Exemplo com duração:

```json
{
  "id": "massage_shoulders",
  "action": "Massage",
  "location": "Shoulders",
  "intensityLevel": 1,
  "execution": { "type": "Seconds", "values": [15, 20, 30] },
  "actorRole": "Opponent",
  "receiverRole": "Winner",
  "accessibility": "OverClothingAllowed",
  "contentTags": ["massage", "upper_body"],
  "cooldownRounds": 4,
  "weight": 1.0,
  "text": "{actor} faz uma massagem nos ombros de {receiver} por {value} segundos"
}
```

Exemplo de ação única:

```json
{
  "id": "gentle_bite_shoulder_once",
  "action": "GentleBite",
  "location": "Shoulder",
  "intensityLevel": 3,
  "execution": { "type": "Repetitions", "values": [1] },
  "actorRole": "Opponent",
  "receiverRole": "Winner",
  "accessibility": "ExposedAreaRequired",
  "requiredClothingState": ["ShirtRemoved"],
  "contentTags": ["bite", "upper_body"],
  "cooldownRounds": 6,
  "weight": 0.7,
  "text": "{actor} dá uma mordida suave no ombro de {receiver}"
}
```

Essa estrutura resolve também o problema da duração. Massagem combina
naturalmente com segundos; uma mordida combina melhor com uma repetição e uma
instrução de intensidade do que com “20 segundos de mordida”.

## 4. Os quatro níveis de intensidade

Os exemplos orientam a curadoria. Eles não devem ser usados como listas livres
para gerar novas combinações. Cada frase final continua sendo revisada e
cadastrada como template completo.

| Nível | Nome | Intenção | Exemplos de famílias | Ritmo típico |
|---:|---|---|---|---|
| 1 | Conexão | quebrar o gelo e iniciar contato leve | abraço, elogio, beijo na testa/bochecha/mãos, carinho no cabelo/rosto, massagem nas mãos/ombros | 5–15 s |
| 2 | Aproximação | aumentar presença e sensualidade | beijo na boca/nuca/pescoço, aproximação sensorial, massagem nos ombros/costas/panturrilhas/pés, carícia nos braços/costas | 10–25 s |
| 3 | Tensão | criar expectativa e contato provocante | beijo prolongado, carícia na cintura/quadril/coxas, massagem nas pernas, mordida suave em templates específicos | 15–35 s ou ação única |
| 4 | Intimidade | representar o estágio mais intenso do catálogo padrão | templates íntimos definidos e aprovados individualmente pela curadoria de produto | definido pelo template |

### 4.1 Guardrails editoriais do catálogo

Os guardrails desta proposta são regras fixas de conteúdo, não configurações de
usuário:

- cada template deve ser revisado como frase completa;
- instruções como “suave” ou “sem deixar marca” devem fazer parte do texto quando
  forem necessárias para caracterizar corretamente a ação;
- rosto, olhos, garganta, articulações, ferimentos e áreas incompatíveis ficam
  fora de templates de mordida ou impacto;
- o nível 4 deve possuir catálogo próprio, sem reutilizar componentes livres dos
  níveis anteriores;
- ações que impeçam a interrupção da experiência não entram no catálogo padrão;
- um template com dúvida editorial permanece inativo até ser revisado.

Essas regras devem impedir a publicação de uma combinação problemática, e não
apenas reduzir sua probabilidade depois que ela já entrou no catálogo.

## 5. Compatibilidade entre ação, local e execução

Cada ação passa a existir somente dentro de templates aprovados. A ausência de
uma combinação significa que ela não pode ser gerada.

Exemplo reduzido de matriz editorial:

| Ação | Combinações possíveis no catálogo | Combinações excluídas |
|---|---|---|
| Beijo | mãos, testa, bochecha, boca, nuca, pescoço e outros templates definidos por nível | olhos e locais sem sentido para a experiência |
| Aproximação sensorial | cabelo, nuca, pescoço e templates de proximidade | nariz como “local do prêmio”, pés ou combinações aleatórias sem intenção |
| Massagem | mãos, ombros, nuca, costas, panturrilhas, pés e pernas | áreas sem técnica coerente ou incompatíveis com a duração |
| Carícia | cabelo, rosto, braços, mãos, ombros, costas, cintura, quadril e coxas | combinações não cadastradas ou incoerentes com o nível |
| Mordida suave | somente templates específicos em locais definidos pela curadoria | nariz, rosto, garganta, articulações e áreas com risco de lesão |
| Lambida | somente templates completos definidos nos níveis correspondentes | qualquer local sorteado genericamente |

“Mordida no nariz” deixa de existir não porque recebeu peso baixo, mas porque não
há um template que permita essa combinação.

## 6. Progressão determinada pelas roupas

O nível da rodada é calculado pelo maior número de peças já perdido por qualquer
jogador:

| Maior quantidade de peças perdidas por um jogador | Nível atual |
|---:|---:|
| 0 | 1 — Conexão |
| 1 | 2 — Aproximação |
| 2 | 3 — Tensão |
| 3 ou 4 | 4 — Intimidade |

Em pseudocódigo:

```text
mostExposedPlayerLosses = max(4 - player.remainingClothesCount)
currentRewardLevel = clamp(1 + mostExposedPlayerLosses, 1, 4)
```

Essa regra garante que:

- toda partida começa no nível 1;
- a primeira peça perdida abre o nível 2;
- a segunda abre o nível 3;
- a terceira abre o nível 4;
- a intensidade nunca sobe mais de um nível numa única resposta;
- erros e acertos contribuem para o estágio por meio da mesma regra de roupas já
  existente.

Como o prêmio é gerado depois da aplicação da resposta, um acerto que provocar a
perda de uma peça já poderá usar o novo nível alcançado.

### 6.1 Curva de entrada em um novo nível

Entrar em um novo estágio não significa usar apenas o novo nível imediatamente.
Para criar uma transição mais natural, o gerador mistura o nível atual com o
anterior:

| Momento no estágio | Peso do nível atual | Peso do nível anterior |
|---|---:|---:|
| Primeiros 2 prêmios gerados após a mudança | 50% | 50% |
| A partir do 3º prêmio no mesmo estágio | 75% | 25% |
| Nível 1 | 100% | não aplicável |

Exemplo: após a primeira peça perdida, os dois primeiros prêmios têm 50% de
chance de ser nível 2 e 50% de permanecer no nível 1. Depois disso, a distribuição
passa a favorecer o nível 2.

O contador considera prêmios **gerados** pelo backend. Os botões “Feito” e “Pular”
continuam com o comportamento atual e não alteram nível, pesos ou histórico de
preferências.

### 6.2 Reinício

Ao reiniciar a partida:

- o nível volta a 1 porque todas as roupas são restauradas;
- o contador de prêmios no estágio volta a zero;
- o histórico de cooldown é limpo;
- a ordem de perguntas continua sendo reembaralhada como ocorre atualmente.

## 7. Algoritmo de seleção

### 7.1 Pipeline obrigatório

Para cada acerto:

1. Aplicar a resposta e eventuais perdas de roupa.
2. Determinar quem oferece e quem recebe o prêmio.
3. Calcular o nível atual com base nas roupas.
4. Escolher o nível-alvo usando os pesos de transição.
5. Carregar templates ativos daquele nível.
6. Remover templates incompatíveis com papéis e estado das roupas.
7. Remover templates ainda dentro do período de cooldown.
8. Aplicar pesos de variedade entre famílias, locais e formas de execução.
9. Sortear um template entre os candidatos restantes.
10. Sortear somente os parâmetros declarados pelo template.
11. Persistir template, nível, parâmetros, participantes e contexto do sorteio.

### 7.2 Pseudocódigo

```text
currentLevel = calculateLevelFromClothing(session)

targetLevel = choosePacedLevel(
    currentLevel,
    session.RewardsGeneratedInCurrentStage
)

candidates = templates
    .where(active)
    .where(level == targetLevel)
    .where(roleRequirementsSatisfied(currentPlayer, opponentPlayer))
    .where(clothingRequirementsSatisfied(session, receiver))
    .where(notInCooldown(session.RecentRewardTemplateIds))

if candidates.empty:
    candidates = repeatFiltersAtLowerLevel()

if candidates.empty:
    return NoRewardAvailable

selected = weightedRandom(
    candidates,
    baseWeight + noveltyWeight + familyBalanceWeight
)

return instantiateOnlyDeclaredParameters(selected)
```

O fallback preserva compatibilidade, requisitos de roupa e guardrails do catálogo.
Ele procura um nível inferior; se ainda não houver candidato, informa que não há
prêmio disponível e permite seguir para a próxima rodada.

### 7.3 Variedade e cooldown

Configuração inicial sugerida:

- não repetir o mesmo template nas próximas 6 rodadas;
- não repetir a mesma família de ação em dois prêmios consecutivos;
- reduzir o peso de uma área usada nos últimos 3 prêmios;
- limpar o cooldown somente no reinício;
- permitir repetição antecipada apenas se o catálogo elegível ficar pequeno,
  preservando nível, compatibilidade e contexto de roupa.

### 7.4 Fórmula de peso sugerida

```text
finalWeight = template.BaseWeight
            × familyNoveltyMultiplier
            × locationNoveltyMultiplier
            × executionNoveltyMultiplier
```

Multiplicadores iniciais:

- família usada no prêmio anterior: `0`;
- local usado nos últimos 3 prêmios: `0,5`;
- execução ainda não usada na partida: `1,25`;
- demais casos: `1`.

O objetivo desses pesos é variedade, não adaptação ao comportamento do jogador.

## 8. Relação entre roupas e acessibilidade

Nível e acessibilidade são conceitos diferentes. Uma área pode ser coerente com
o nível atual, mas ainda estar coberta pela roupa do jogador que receberá o
prêmio.

Cada template declara uma destas opções:

- `Any`: independe da roupa;
- `OverClothingAllowed`: funciona mesmo com a área coberta;
- `ExposedAreaRequired`: só entra no sorteio quando o estado da sessão indicar que
  a peça correspondente foi perdida.

Também pode declarar requisitos explícitos:

```text
RequiredClothingState
- SocksRemoved
- ShirtRemoved
- PantsRemoved
- UnderwearRemoved
```

O filtro deve considerar as roupas do **recebedor** do prêmio, não apenas do
jogador punido naquela resposta. Se o recebedor ainda estiver com a peça exigida,
o template não entra no conjunto elegível.

## 9. Papéis claros no prêmio

Hoje, “Jogador ganhou: massagem nas costas” deixa implícito quem faz e quem recebe.
Todo template deve declarar:

- `ActorRole`: quem executa;
- `ReceiverRole`: quem recebe;

Regra padrão recomendada:

```text
o adversário oferece o prêmio; quem acertou recebe
```

O texto final deve usar nomes e voz direta:

```text
Alex faz uma massagem nos ombros de Dani por 20 segundos.
```

Isso reduz dúvida e interpretações diferentes entre os dois aparelhos.

## 10. Modelo de dados sugerido

### 10.1 Template de prêmio

```text
RewardTemplate
- Id
- Name
- TextTemplate
- Action
- Location
- IntensityLevel (1..4)
- ExecutionType (Seconds, Repetitions, FreeForm)
- AllowedExecutionValues
- ActorRole
- ReceiverRole
- Accessibility
- RequiredClothingState
- ContentTags
- CooldownRounds
- BaseWeight
- Active
```

### 10.2 Estado da progressão

```text
GameRewardProgression
- GameSessionId
- CurrentLevel
- RewardsGeneratedInCurrentStage
- RecentTemplateIds
- RecentActionFamilies
- RecentLocations
```

`CurrentLevel` pode ser recalculado pelas roupas, mas persistir o valor facilita
detectar a entrada em um novo estágio e zerar o contador correspondente.

### 10.3 Histórico do prêmio

```text
RewardHistory
- Id
- GameSessionId
- TemplateId
- IntensityLevel
- ActorPlayerId
- ReceiverPlayerId
- RenderedText
- ExecutionType
- ExecutionValue
- ClothingStateSnapshot
- CreatedAt
```

Esse histórico registra somente a geração e o contexto técnico do prêmio. Não há
coleta de preferências, respostas sobre o prêmio ou decisões do jogador neste
escopo.

## 11. Exemplo de progressão

| Momento | Estado das roupas | Distribuição | Possível prêmio |
|---|---|---|---|
| Início | ninguém perdeu peça | 100% nível 1 | “Rafa faz uma massagem nas mãos de Bia por 10 segundos.” |
| Primeira peça perdida | maior perda individual = 1 | 50% nível 1 / 50% nível 2 | “Bia dá um beijo demorado na nuca de Rafa.” |
| Permanência no estágio | terceiro prêmio após a primeira perda | 25% nível 1 / 75% nível 2 | “Rafa faz uma massagem nas costas de Bia por 20 segundos.” |
| Segunda peça perdida | maior perda individual = 2 | 50% nível 2 / 50% nível 3 | “Bia faz uma massagem nas pernas de Rafa por 30 segundos.” |
| Terceira peça perdida | maior perda individual = 3 | 50% nível 3 / 50% nível 4 | template curado de nível 4 compatível com roupas e cooldown |

A curva é automática e reproduzível a partir do estado da sessão. Nenhuma etapa
depende de formulário, votação ou preferência coletada durante a partida.

## 12. Tratamento do prêmio final

Um acerto que encerra a partida gera e registra um prêmio. Foi implementada a
primeira opção avaliada: a interface mostra esse prêmio final antes da tela de
vencedor. Isso preserva a regra “todo acerto gera prêmio” e permite que o nível 4
apareça no fechamento da partida.

## 13. Estratégia de implementação adotada

### Fase 1 — Coerência mínima — implementada

- substituir os três arrays pelo catálogo de templates;
- adicionar nível, papéis, execução, requisitos de roupa e cooldown;
- cadastrar e revisar templates de todos os níveis;
- impedir combinações não cadastradas;
- registrar `TemplateId` no histórico.

Essa fase já elimina casos como “mordida no nariz”.

### Fase 2 — Progressão — implementada

- calcular o nível pelas peças perdidas;
- persistir contador de prêmios gerados no estágio;
- implementar distribuição 50/50 e depois 75/25;
- zerar estágio e cooldown no reinício;
- decidir e implementar o fluxo do prêmio final.

### Fase 3 — Contexto e variedade — implementada

- filtrar por roupa e papel do recebedor;
- aplicar cooldown de template, bloqueio da última família e peso reduzido para
  locais recentes;
- implementar pesos de novidade;
- garantir fallback entre o nível atual e o anterior, sem relaxar curadoria,
  papéis ou requisitos de roupa.

### Fase 4 — Observabilidade e curadoria — parcial

- medir distribuição de templates, níveis, famílias e locais;
- identificar templates raramente elegíveis por requisitos excessivos;
- criar validação automatizada do catálogo no startup ou CI;
- revisar pesos sem alterar o nível calculado pela partida;
- ampliar o catálogo mantendo cada prêmio como unidade editorial completa.

## 14. Critérios de aceite

Uma implementação pode ser considerada correta quando:

- “mordida no nariz” e qualquer combinação fora da lista de templates forem
  impossíveis, não apenas improváveis;
- o nível for 1, 2, 3 ou 4 conforme a maior perda individual de 0, 1, 2 ou 3+
  peças;
- nenhuma resposta elevar o nível em mais de uma etapa;
- os dois primeiros prêmios de um estágio usarem pesos 50/50 e os seguintes
  75/25 entre nível atual e anterior;
- o mesmo template respeitar o cooldown configurado;
- requisitos de roupa forem verificados contra o recebedor do prêmio;
- papéis e texto não deixarem dúvida sobre quem oferece e quem recebe;
- o fallback nunca gerar componente ou combinação não cadastrada;
- o reinício limpar nível, contadores e cooldown;
- nenhum endpoint, formulário ou entidade de preferência do jogador for criado
  como parte desta entrega;
- testes cobrirem catálogo, compatibilidade, progressão, pesos, fallback e
  concorrência.

## 15. Parâmetros adotados e revisão de produto

A implementação adotou pelo menos 12 templates ativos por nível, catálogo inicial
com 49 templates, execução em segundos/repetições/texto, pesos e cooldowns por
template, requisitos de roupa explícitos e exibição do prêmio final. O catálogo
possui validação automatizada e impede a ativação se não atender à cobertura
mínima ou contiver pares proibidos.

Antes de ligar a flag em produção, produto ainda deve homologar a linguagem
editorial e os parâmetros iniciais do catálogo. Essa homologação pode ajustar
conteúdo, peso, cooldown e valores de execução sem alterar o algoritmo ou o
contrato da API.

Essas decisões pertencem à mecânica dos prêmios. Os temas reservados ao épico de
configuração e requisitos jurídicos permanecem separados.

## 16. Decisão recomendada

A melhor direção é combinar **curadoria forte** com **sorteio controlado**. Um
modelo puramente generativo ou um produto cartesiano continuará encontrando
combinações estranhas. O sistema pode parecer inteligente usando filtros, níveis,
pesos e contexto, mas a unidade de conteúdo deve continuar sendo um prêmio
completo, legível e previamente aprovado.

Em resumo:

```text
roupas determinam o estágio
+ templates garantem coerência
+ transições 50/50 e 75/25 suavizam a curva
+ contexto de roupa mantém o prêmio executável
+ pesos e cooldown mantêm surpresa
```
