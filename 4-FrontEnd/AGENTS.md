AGENTS.md

CloudShopping — Regras para o Front-End

Este documento define as regras obrigatórias que qualquer agente de IA deve seguir ao analisar, criar, alterar, corrigir ou refatorar código dentro da aplicação Front-End do CloudShopping.

Este arquivo deve ser colocado na raiz do projeto Front-End, por exemplo:

CloudShopping/
└── 4-FrontEnd/
    └── AGENTS.md

Estas instruções complementam as regras arquiteturais e de negócio existentes no Backend.

O Front-End não deve recriar ou contradizer regras de negócio que pertencem ao Domain ou Application do Backend.

«IMPORTANTE: antes de alterar qualquer arquivo do Front-End, analise a estrutura atual do projeto, componentes existentes, hooks, services, contratos da API, padrões utilizados e funcionalidades relacionadas.

NÃO invente endpoints, propriedades, DTOs, hooks, componentes, campos ou regras de negócio sem verificar primeiro a implementação existente.

NÃO reestruture módulos inteiros quando a tarefa exige apenas uma alteração localizada.»

---

1. Objetivo do Front-End

O Front-End é responsável por:

- experiência do usuário;
- apresentação de informações;
- navegação;
- formulários;
- validações de entrada para UX;
- gerenciamento de estado de interface;
- comunicação com o Backend;
- autenticação;
- sessão de visitante;
- gerenciamento do carrinho;
- cache de dados;
- tratamento de loading;
- tratamento de erros;
- feedback visual;
- responsividade;
- acessibilidade;
- segurança no cliente;
- consistência visual.

O Front-End NÃO é responsável por definir a regra definitiva do negócio.

A regra definitiva deve permanecer no Backend.

---

2. Stack Principal

Manter a stack definida pelo projeto.

React
TypeScript
Vite
Tailwind CSS
React Query / TanStack Query
Axios

Não adicionar frameworks alternativos sem necessidade explícita.

Evitar introduzir paralelamente:

Redux
MobX
Angular
Vue
jQuery
Bootstrap
outro HTTP client

quando já existe uma solução adequada no projeto.

---

3. Princípio Arquitetural

Fluxo preferencial:

Page
   |
   v
Component
   |
   v
Hook
   |
   v
Service
   |
   v
HTTP Client
   |
   v
Backend API

Responsabilidades devem estar separadas.

Evitar:

Component
   |
   +--> Axios
   +--> localStorage
   +--> regra de negócio
   +--> manipulação de token
   +--> tratamento global de erro

Componentes devem permanecer focados em interface e interação.

---

4. Antes de Alterar Código

O agente DEVE:

1. localizar o arquivo alvo;
2. entender sua responsabilidade;
3. localizar componentes relacionados;
4. localizar hooks relacionados;
5. localizar services relacionados;
6. localizar tipos/interfaces utilizados;
7. localizar chamadas à API;
8. verificar autenticação;
9. verificar Tenant;
10. verificar Guest/SessionToken;
11. verificar comportamento mobile;
12. verificar padrão visual existente;
13. procurar implementação semelhante;
14. executar a menor mudança possível.

---

5. Não Inventar Estrutura

Antes de criar:

novo componente;
novo hook;
novo service;
novo context;
novo provider;
nova interface;
nova pasta;
novo endpoint;

verifique se já existe equivalente.

Evitar duplicações como:

useCart
useShoppingCart
useCartData
useCurrentCart

para resolver o mesmo problema.

---

6. Organização por Responsabilidade

Quando compatível com a estrutura atual do projeto, manter separação semelhante a:

src/
├── components/
├── pages/
├── hooks/
├── services/
├── api/
├── contexts/
├── types/
├── utils/
├── layouts/
├── routes/
└── assets/

Se o projeto estiver estruturado por feature, preservar o padrão existente.

Não reorganizar o projeto inteiro apenas para adequá-lo a uma preferência do agente.

---

7. Organização por Feature

Quando uma funcionalidade cresce, preferir agrupamento por domínio funcional.

Exemplo:

features/
├── auth/
├── customers/
├── products/
├── cart/
├── checkout/
├── orders/
└── payments/

Cada feature pode possuir:

components/
hooks/
services/
types/
utils/

desde que isso esteja alinhado à estrutura existente.

---

8. Componentes

Componentes devem ser:

- pequenos;
- focados;
- reutilizáveis quando necessário;
- previsíveis;
- tipados;
- fáceis de testar;
- sem efeitos colaterais inesperados.

Um componente não deve acumular diversas responsabilidades.

---

9. Componentes de UI

Componentes puramente visuais devem receber dados por "props".

Exemplo:

<ProductCard
  product={product}
  onAddToCart={handleAddToCart}
/>

Preferir isso a fazer o próprio "ProductCard" buscar produto, consultar carrinho, acessar token e executar chamadas HTTP.

---

10. Pages

Pages podem coordenar:

- hooks;
- dados;
- layout;
- navegação;
- componentes.

Mas não devem concentrar toda a lógica da aplicação.

Evitar páginas com centenas de linhas misturando:

JSX;
requisições;
transformações;
regras;
storage;
validação;
tratamento de erro.

---

11. Hooks

Hooks são responsáveis por encapsular comportamento reutilizável do Front-End.

Exemplos:

useAuth
useGuest
useCart
useProducts
useOrders
useCheckout
useCustomer

Um hook não deve existir apenas para envolver uma única linha sem agregar semântica ou reutilização.

---

12. Regras dos Hooks

Hooks devem:

- começar com "use";
- obedecer às Rules of Hooks;
- evitar efeitos colaterais escondidos;
- possuir retorno previsível;
- ser corretamente tipados;
- delegar HTTP a services/API clients quando possível.

Evitar "useEffect" como solução padrão para qualquer problema.

---

13. useEffect

Use "useEffect" apenas para sincronização com sistemas externos ou efeitos reais.

Evitar utilizar "useEffect" para:

- derivar estado;
- filtrar arrays;
- calcular valores;
- sincronizar dois estados React desnecessariamente;
- executar lógica que pode ocorrer diretamente em evento.

Preferir:

const total = items.reduce(...);

em vez de:

useEffect(() => {
  setTotal(items.reduce(...));
}, [items]);

---

14. Estado Derivado

Não armazenar estado que pode ser calculado diretamente.

Evitar:

const [cartCount, setCartCount] = useState(0);

se já existe:

const cartCount = cart.items.length;

Estado duplicado gera inconsistências.

---

15. Estado Local

Utilizar "useState" para estado realmente local da interface.

Exemplos:

modal aberto;
tab selecionada;
campo temporário;
drawer aberto;
accordion.

Não utilizar Context global para estado que pertence a apenas um componente.

---

16. Estado Global

Utilizar estado global somente quando diferentes áreas da aplicação realmente precisam compartilhar o mesmo estado.

Exemplos possíveis:

autenticação;
usuário atual;
Tenant;
sessão Guest;
preferências globais;
carrinho, quando necessário.

Não globalizar tudo.

---

17. React Query

Dados provenientes do servidor devem preferencialmente ser gerenciados pelo React Query.

Isso inclui:

Products
Customers
Cart
Orders
Payments
Categories
Addresses

Evitar copiar indiscriminadamente dados do React Query para "useState".

---

18. Query Keys

Query Keys devem ser previsíveis e consistentes.

Exemplo:

["products"]
["product", productId]
["cart", sessionToken]
["orders", customerId]

Para multi-tenancy, incluir o Tenant quando necessário para impedir colisão de cache.

Exemplo:

["products", tenantId]

---

19. Cache Multi-Tenant

É proibido reutilizar cache de dados entre Tenants diferentes.

Ao trocar de Tenant:

- limpar ou invalidar dados sensíveis;
- garantir Query Keys isoladas;
- evitar exibição temporária de dados do Tenant anterior.

---

20. Queries

Queries devem ser usadas para leitura.

Exemplo:

useQuery({
  queryKey: ["products", tenantId],
  queryFn: () => productService.list(...)
});

Não executar mutation através de Query apenas para aproveitar cache.

---

21. Mutations

Operações que alteram estado no servidor devem utilizar "useMutation".

Exemplos:

criar;
editar;
excluir;
adicionar ao carrinho;
checkout;
login;
pagamento.

Após sucesso:

- atualizar cache;
- invalidar queries relevantes;
- exibir feedback apropriado.

---

22. Atualização do Cache

Depois de mutation, escolher conscientemente entre:

invalidateQueries
setQueryData
refetchQueries

Não invalidar a aplicação inteira.

Evitar:

queryClient.invalidateQueries();

sem filtro quando somente o carrinho mudou.

---

23. Optimistic Update

Utilizar atualização otimista somente quando:

- melhora realmente a experiência;
- existe rollback correto;
- comportamento de falha é tratado;
- consistência não fica comprometida.

Pagamento e operações financeiras geralmente não devem fingir sucesso antes da confirmação real.

---

24. Axios

Axios deve estar centralizado em um HTTP client configurado.

Evitar:

axios.get(...)

diretamente em componentes.

Preferir:

Component
   ↓
Hook
   ↓
Service
   ↓
apiClient

---

25. API Client

O cliente Axios central pode ser responsável por:

- "baseURL";
- headers comuns;
- autenticação;
- "X-Tenant-ID";
- interceptors;
- timeout;
- tratamento técnico de erros.

Não duplicar essa configuração em cada service.

---

26. Base URL

Nunca hardcodar URL da API em componente.

Evitar:

fetch("http://localhost:8080/api/products");

Preferir variável de ambiente.

Exemplo:

VITE_API_BASE_URL

---

27. Variáveis de Ambiente

Variáveis client-side devem ser tratadas como públicas.

Nunca colocar em "VITE_*":

senha;
secret;
private key;
Asaas API Key;
token administrativo permanente;
connection string.

Tudo enviado ao bundle Front-End pode ser inspecionado pelo usuário.

---

28. Services

Services devem representar operações da API.

Exemplo:

productService.list()
productService.getById(id)

cartService.get()
cartService.addItem(request)

authService.login(request)

Não colocar regra de renderização dentro do service.

---

29. Contratos da API

Tipos do Front-End devem refletir contratos reais do Backend.

Antes de alterar:

interface Product {}

verifique DTO/response real da API.

Não adicionar propriedade fictícia para resolver erro de TypeScript.

---

30. DTOs

Diferenciar quando necessário:

Request
Response
ViewModel
FormData

Não usar a mesma interface para tudo quando os formatos são diferentes.

Exemplo:

CreateCustomerRequest
CustomerResponse
CustomerFormData

---

31. TypeScript

TypeScript deve ser utilizado de forma estrita.

Evitar:

any

Sempre que possível.

Não resolver erro de compilação adicionando "as any".

---

32. unknown

Para dados externos desconhecidos, preferir:

unknown

e realizar narrowing correto.

Não assumir formato apenas por conveniência.

---

33. Type Assertions

Evitar casts como:

value as Product

quando não há garantia de que o valor realmente possui aquele formato.

Type assertion não valida dados em runtime.

---

34. Props

Props devem possuir interfaces/tipos claros.

Exemplo:

interface ProductCardProps {
  product: Product;
  onAddToCart: (productId: string) => void;
}

Evitar props genéricas demais.

---

35. Nullable

Respeitar "null" e "undefined".

Não utilizar:

value!

apenas para silenciar TypeScript.

O operador non-null assertion deve ser exceção.

---

36. Optional Chaining

Utilizar:

customer?.address?.city

quando ausência for comportamento válido.

Mas não esconda erro de estado obrigatório usando optional chaining indiscriminadamente.

---

37. Enums e Constantes

Não espalhar magic strings pela aplicação.

Evitar:

if (status === "PAID")

em dezenas de arquivos.

Preferir constante ou tipo central quando apropriado.

---

38. Regras de Negócio

O Front-End pode possuir regras de apresentação.

Exemplo:

mostrar botão;
desabilitar ação;
formatar preço;
mostrar estado visual.

Mas não deve ser a única proteção para regra de negócio.

Exemplo:

if (stock === 0) {
  disableButton();
}

Isso melhora UX.

Mas o Backend ainda deve impedir compra sem estoque.

---

39. Validação de Formulário

Validação client-side existe para melhorar UX.

Ela NÃO substitui validação do Backend.

Sempre tratar resposta de validação do servidor.

---

40. Mensagens de Validação

Apresentar mensagens próximas aos campos.

Evitar apenas:

Algo deu errado

quando o Backend fornece informação útil e segura.

---

41. Erros da API

Centralizar normalização de erros quando possível.

Exemplo conceitual:

AxiosError
   ↓
API Error Mapper
   ↓
Application Error
   ↓
UI

Componentes não deveriam interpretar manualmente dezenas de formatos diferentes de erro.

---

42. HTTP 400

Tratar como erro de entrada ou regra conforme contrato da API.

Não mostrar automaticamente mensagem técnica do servidor.

---

43. HTTP 401

Normalmente indica autenticação inválida ou expirada.

O Front-End pode:

- invalidar sessão;
- limpar credenciais locais;
- redirecionar para login;
- preservar rota de retorno quando fizer sentido.

Evitar loops de redirecionamento.

---

44. HTTP 403

Não tratar "403" como "401".

401 -> não autenticado
403 -> autenticado, porém sem autorização

Mostrar comportamento apropriado.

---

45. HTTP 404

404 deve possuir tratamento coerente.

Exemplos:

Produto não encontrado
Pedido não encontrado
Página não encontrada

Não deixar exceção Axios aparecer diretamente na tela.

---

46. HTTP 409

Conflitos podem representar:

duplicidade;
estado incompatível;
concorrência;
operação já realizada.

Mostrar mensagem coerente com o código de erro da API.

---

47. HTTP 500

Não exibir stack trace ou detalhes internos.

Mostrar mensagem segura.

Registrar contexto quando existir infraestrutura de observabilidade.

---

48. Loading

Toda operação assíncrona visível deve possuir estado de loading apropriado.

Exemplos:

Skeleton
Spinner
Botão desabilitado
Placeholder

Evitar tela vazia sem feedback.

---

49. Loading de Mutation

Ao enviar formulário:

- impedir envio duplicado;
- indicar processamento;
- reabilitar ação quando necessário.

Exemplo:

<button disabled={mutation.isPending}>

---

50. Empty State

Listas vazias devem possuir estado próprio.

Exemplos:

Nenhum produto encontrado
Seu carrinho está vazio
Você ainda não possui pedidos

Empty State não é erro.

---

51. Error State

Erro deve ser visualmente diferente de Empty State.

Quando possível oferecer:

Tentar novamente
Voltar
Atualizar

sem criar loop de requisições.

---

52. Autenticação

A autenticação deve estar centralizada.

Evitar cada componente consultar token diretamente.

Preferir:

AuthProvider
useAuth
apiClient

ou padrão equivalente já existente.

---

53. JWT

O Front-End pode armazenar e enviar JWT conforme arquitetura definida.

Mas nunca deve:

- confiar em claims para segurança definitiva;
- decidir autorização definitiva;
- modificar token;
- criar JWT;
- guardar segredo usado para assinar JWT.

---

54. Token Expirado

O comportamento de expiração deve ser previsível.

Exemplo:

API retorna 401
     ↓
limpar autenticação
     ↓
limpar caches sensíveis
     ↓
redirecionar login

Não continuar exibindo dados privados em cache depois do logout.

---

55. Logout

Logout deve limpar:

- token;
- usuário atual;
- dados privados em cache;
- informações sensíveis da sessão.

Não necessariamente deve apagar dados Guest se a regra de negócio determinar continuidade da experiência.

---

56. Multi-Tenancy

CloudShopping é multi-tenant.

O Front-End deve propagar corretamente:

X-Tenant-ID

quando exigido pela API.

Não hardcodar Tenant em services individuais.

---

57. Tenant Atual

A origem do Tenant deve ser centralizada.

Pode ser:

subdomínio;
rota;
configuração;
contexto;
sessão.

conforme arquitetura real.

Não permitir que diferentes componentes determinem Tenant de maneiras diferentes.

---

58. Mudança de Tenant

Ao mudar Tenant:

- invalidar contexto anterior;
- limpar dados incompatíveis;
- atualizar headers;
- atualizar queries;
- garantir que carrinho/sessão pertençam ao Tenant correto.

Evitar vazamento visual de dados entre lojas.

---

59. Branch

Se a aplicação utiliza Branch:

- manter "BranchId" separado de "TenantId";
- nunca tratá-los como sinônimos;
- propagar Branch somente onde o contrato exige;
- atualizar cache quando a filial mudar.

---

60. Guest

Usuários não autenticados podem utilizar sessão Guest.

Responsabilidades incluem:

obter sessão;
armazenar token apropriado;
identificar carrinho;
manter continuidade de navegação.

---

61. useGuest

Se o projeto já possui "useGuest", ele deve ser a principal abstração para comportamento de visitante.

Não duplicar lógica de Guest em:

Header
CartDrawer
ProductPage
Checkout

---

62. SessionToken

"SessionToken" deve possuir uma única fonte confiável no Front-End.

Evitar gerar novos tokens durante render.

Errado:

const token = crypto.randomUUID();

dentro de um componente executado repetidamente.

---

63. VisitorToken

Quando existir "VisitorToken", respeitar o contrato existente entre Backend e Front-End.

Não assumir automaticamente que:

VisitorToken == SessionToken == JWT

Podem representar conceitos diferentes.

---

64. Carrinho

O carrinho é funcionalidade crítica.

A implementação deve suportar corretamente:

Guest;
Customer;
SessionToken;
Tenant;
adição;
atualização;
remoção;
expiração;
sincronização.

---

65. CartDrawer

"CartDrawer" deve permanecer predominantemente componente de apresentação/interação.

Evitar concentrar nele:

- acesso direto à API;
- criação da sessão Guest;
- regra de preço;
- autenticação;
- cálculo complexo;
- armazenamento manual de token.

Delegar para hooks/services.

---

66. Contador do Carrinho

O contador do Header deve utilizar uma fonte única do estado do carrinho.

Evitar manter:

contador no Header
+
contador no Drawer
+
contador no localStorage

como estados independentes.

---

67. Atualização do Carrinho

Depois de:

AddItem
UpdateQuantity
RemoveItem

garantir sincronização do cache do carrinho.

Não depender de reload da página para refletir alteração.

---

68. Quantidade

A UI deve impedir valores obviamente inválidos.

Exemplo:

0
negativo
NaN

Mas Backend continua sendo autoridade final.

---

69. Estoque

Não confiar apenas no estoque carregado anteriormente.

O estoque pode mudar entre:

visualização;
carrinho;
checkout.

Tratar conflitos retornados pelo Backend.

---

70. Preços

Preço exibido deve vir de fonte confiável.

Não enviar preço calculado no navegador esperando que o Backend confie nesse valor.

O Backend deve calcular/validar o valor final.

---

71. Money

Evitar cálculos monetários frágeis no Front-End.

Não utilizar float para criar regra financeira crítica.

Front-End pode formatar valores recebidos.

---

72. Formatação Monetária

Utilizar APIs adequadas.

Exemplo:

new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

Evitar concatenação manual:

"R$ " + price

quando isso gerar inconsistência.

---

73. Datas

Utilizar formatação explícita.

Não assumir timezone do Backend.

Evitar parsing ambíguo.

---

74. localStorage

"localStorage" deve ser usado com cuidado.

Não espalhar chamadas diretas em vários componentes.

Preferir abstração central.

---

75. Dados Sensíveis no Storage

Nunca armazenar em "localStorage":

senha;
API Key;
segredo;
dados financeiros sensíveis;
private key.

Tokens também exigem análise de risco conforme arquitetura de autenticação existente.

---

76. sessionStorage

Usar "sessionStorage" apenas quando o ciclo de vida por aba/sessão fizer sentido.

Não usar por conveniência aleatória.

---

77. Cookies

Se autenticação utiliza cookies:

- respeitar configuração Backend;
- não tentar manipular cookie HttpOnly via JavaScript;
- configurar Axios corretamente;
- considerar CSRF quando aplicável.

---

78. Segurança

Nunca considerar o Front-End uma barreira de segurança definitiva.

Usuário pode modificar:

HTML;
JavaScript;
requests;
headers;
storage;
estado React.

Toda proteção importante deve existir no servidor.

---

79. XSS

Evitar:

dangerouslySetInnerHTML

a menos que absolutamente necessário.

Se necessário, conteúdo deve ser sanitizado corretamente.

---

80. Dados Externos

Nunca confiar cegamente em HTML fornecido por API ou usuário.

Texto deve ser tratado como texto.

---

81. Open Redirect

Ao aceitar URL de retorno após login, validar destinos permitidos.

Não redirecionar diretamente para URL arbitrária recebida via query string.

---

82. Links Externos

Quando utilizar:

target="_blank"

considerar:

rel="noopener noreferrer"

quando apropriado.

---

83. Acessibilidade

Todo componente interativo deve ser acessível.

Considerar:

- teclado;
- foco;
- labels;
- contraste;
- ARIA quando necessário;
- semântica HTML.

---

84. HTML Semântico

Preferir:

button
nav
main
section
article
label
form

em vez de transformar "div" em tudo.

---

85. Botões

Ação clicável deve preferencialmente utilizar:

<button>

e não:

<div onClick={...}>

Isso melhora acessibilidade e comportamento padrão.

---

86. Formulários

Campos devem possuir "label" acessível.

Não utilizar placeholder como única identificação de campo.

---

87. Imagens

Imagens relevantes devem possuir "alt".

Imagens puramente decorativas podem utilizar:

alt=""

conforme contexto.

---

88. Keyboard Navigation

Modais, menus, drawers e dropdowns devem funcionar com teclado quando aplicável.

Não criar controles utilizáveis apenas com mouse.

---

89. Focus

Ao abrir modal/drawer, considerar foco.

Ao fechar, restaurar foco quando apropriado.

---

90. Responsividade

A aplicação deve funcionar em:

mobile;
tablet;
desktop.

Não corrigir desktop quebrando mobile.

---

91. Tailwind

Manter Tailwind conforme padrão do projeto.

Evitar CSS inline indiscriminado.

Preferir classes Tailwind ou abstrações existentes.

---

92. Classes Excessivas

Quando uma combinação de classes se repete frequentemente, considerar componente reutilizável.

Mas não criar abstração prematura para duas ocorrências triviais.

---

93. Breakpoints

Utilizar breakpoints existentes do Tailwind.

Não criar media queries aleatórias quando classes responsivas resolvem.

---

94. Design Consistente

Antes de criar novo padrão visual, procurar componentes existentes.

Reutilizar:

Button
Input
Modal
Drawer
Card
Badge
Table
Spinner
Alert

quando existirem.

---

95. Botões

A aplicação deve possuir estilos consistentes para:

primary;
secondary;
danger;
ghost;
disabled;
loading.

Não inventar cor e comportamento novo em cada tela.

---

96. Cores

Preferir tokens/classes já utilizadas.

Não espalhar valores hexadecimais aleatórios.

---

97. Tipografia

Manter hierarquia consistente:

Page title
Section title
Card title
Body
Caption
Error

---

98. Ícones

Utilizar biblioteca existente.

Não adicionar segunda biblioteca de ícones sem necessidade.

---

99. Duplicação de Componentes

Antes de criar:

ProductCard2
NewProductCard
ProductCardNew

analise possibilidade de evoluir o componente existente.

---

100. Componentes Genéricos Demais

Evitar componentes como:

UniversalComponent
GenericContainer
DynamicEverything

com dezenas de props.

Composição geralmente é melhor.

---

101. Props Booleanas Excessivas

Evitar:

<Component
  isAdmin
  isSmall
  isBlue
  isModal
  isCheckout
  showX
  hideY
/>

quando isso indica responsabilidades distintas.

Considere variantes ou componentes especializados.

---

102. Memoização

Não utilizar automaticamente:

useMemo
useCallback
React.memo

em todo componente.

Use quando houver benefício real ou necessidade de identidade estável.

Memoização desnecessária aumenta complexidade.

---

103. Performance

Antes de otimizar, identificar problema real.

Possíveis pontos:

re-render;
lista grande;
imagem pesada;
requisições duplicadas;
bundle grande;
query excessiva.

Não sacrificar clareza por micro-otimização sem evidência.

---

104. Listas

Utilizar "key" estável.

Preferir:

key={product.id}

Evitar índice:

key={index}

quando lista pode ser reordenada ou modificada.

---

105. Imagens de Produtos

Evitar carregar imagens enormes quando versão otimizada estiver disponível.

Utilizar lazy loading quando apropriado.

---

106. Code Splitting

Rotas ou áreas pesadas podem utilizar lazy loading.

Não dividir componentes minúsculos apenas para dizer que existe code splitting.

---

107. Imports

Manter imports organizados conforme padrão do projeto.

Remover imports não utilizados.

Não introduzir alias novo sem necessidade.

---

108. Console

Não deixar:

console.log(...)

de debug em produção.

Logs necessários devem utilizar solução apropriada se existente.

---

109. Comentários

Comentários devem explicar decisões, não repetir código.

Ruim:

// seta loading como true
setLoading(true);

Útil:

// Mantém a sessão Guest durante o login para permitir merge do carrinho.

---

110. TODO

Não adicionar "TODO" genérico sem contexto.

Quando necessário, explicar claramente a dívida ou ação futura.

---

111. Código Morto

Remover:

- imports não usados;
- funções não usadas;
- estados abandonados;
- componentes substituídos.

Não manter código comentado por longos blocos.

Git já mantém histórico.

---

112. Nomenclatura

Utilizar nomes claros em inglês para código, conforme padrão técnico do projeto.

Exemplos:

ProductCard
CartDrawer
useGuest
createOrder
customerId
sessionToken

Textos exibidos para usuário podem permanecer em português.

---

113. Booleanos

Booleans devem indicar claramente estado.

Preferir:

isLoading
isAuthenticated
hasItems
canCheckout

Evitar:

status
flag
value
check

---

114. Handlers de Eventos

Utilizar nomes claros:

handleSubmit
handleAddToCart
handleRemoveItem
handleLogin

Callbacks recebidos por props podem usar:

onSubmit
onAddToCart
onClose

---

115. Funções Assíncronas

Sempre tratar sucesso e falha adequadamente.

Não deixar Promise rejeitada sem tratamento quando a biblioteca não gerencia isso.

---

116. Double Submit

Operações críticas devem impedir envio repetido.

Especialmente:

checkout;
pagamento;
criação de pedido;
cadastro;
login.

---

117. Idempotência

Quando Backend oferece chave de idempotência para operação crítica, Front-End deve utilizá-la conforme contrato.

Não gerar nova chave a cada retry automático da mesma ação quando isso resultar em duplicidade.

---

118. Checkout

Checkout deve manter separação entre:

dados do cliente;
endereço;
carrinho;
frete;
pagamento;
confirmação.

Evitar componente monolítico com toda a jornada em um único arquivo.

---

119. Pedido

Depois da criação do pedido:

- utilizar identificador retornado pelo Backend;
- não assumir ID;
- invalidar caches apropriados;
- tratar falha de forma consistente.

---

120. Pagamento

Nunca considerar pagamento confirmado apenas porque o usuário clicou em pagar.

Estado deve refletir confirmação real do Backend/provedor.

---

121. Status de Pagamento

Estados como:

Pending
Processing
Paid
Failed
Cancelled
Refunded

devem refletir contrato real.

Não criar novos estados apenas para UI sem mapeamento definido.

---

122. Polling

Polling só deve existir quando necessário.

React Query pode utilizar:

refetchInterval

quando apropriado.

Não criar "setInterval" espalhado por componentes.

---

123. Webhooks

Webhooks são processados pelo Backend.

Front-End não recebe webhook diretamente como regra padrão.

A UI consulta o estado resultante via API.

---

124. Rotas

Rotas devem permanecer centralizadas.

Evitar strings duplicadas como:

"/products"
"/cart"
"/checkout"
"/orders"

espalhadas por dezenas de componentes quando o projeto possui abstração de routes.

---

125. Rotas Protegidas

Rotas administrativas ou autenticadas devem possuir proteção visual.

Mas proteção real continua no Backend.

Esconder menu não equivale a autorização.

---

126. Redirecionamentos

Redirecionamentos devem ser previsíveis.

Exemplo:

não autenticado
   ↓
login
   ↓
retorno para página anterior

quando apropriado.

Evitar loops.

---

127. Página 404

Rotas inexistentes devem possuir página de fallback amigável.

Não deixar tela branca.

---

128. Administração

Interfaces administrativas devem respeitar:

- autorização;
- Tenant;
- Branch;
- loading;
- erros;
- paginação;
- filtros;
- confirmação para operações destrutivas.

---

129. Operações Destrutivas

Exclusões e cancelamentos importantes devem exigir confirmação apropriada.

Não executar ação destrutiva simplesmente ao clicar em ícone sem feedback.

---

130. Soft Delete

Se o Backend utiliza Soft Delete, a UI deve refletir isso conforme contrato.

Não assumir remoção física.

Itens inativos podem exigir filtros ou estados diferentes.

---

131. Paginação

Listas grandes devem utilizar paginação quando suportada pela API.

Não buscar milhares de registros apenas para paginar no navegador sem necessidade.

---

132. Filtros

Filtros server-side devem ser enviados à API quando essa for a arquitetura.

Evitar carregar tudo e filtrar localmente em grandes datasets.

---

133. Busca

Busca digitada pode utilizar debounce quando apropriado.

Não disparar request a cada tecla sem necessidade.

---

134. Debounce

Debounce deve possuir tempo razoável e cancelamento correto.

Não criar múltiplas implementações diferentes pelo projeto.

---

135. Abort / Cancellation

Quando possível, requisições que deixam de ser necessárias devem ser canceladas.

Especialmente:

busca;
autocomplete;
mudança rápida de rota;
filtros.

---

136. Race Conditions

Considerar cenários onde respostas chegam fora de ordem.

React Query e cancellation devem ser utilizados adequadamente.

Não deixar resultado antigo substituir resultado mais recente.

---

137. Forms

Não duplicar o mesmo valor em múltiplos estados sem necessidade.

Preferir fonte única.

---

138. Normalização

Normalizar valores quando necessário.

Exemplos:

trim;
telefone;
documento;
CEP.

Mas preservar comportamento esperado do Backend.

---

139. CPF/CNPJ

Validação visual pode existir no Front-End.

Backend deve continuar validando oficialmente.

Não confiar apenas em máscara.

---

140. Máscaras

Máscara é apresentação.

Valor enviado ao Backend deve seguir contrato definido.

Não assumir automaticamente que API espera valor com pontuação.

---

141. CEP

Tratamento de CEP deve considerar:

- loading;
- inválido;
- não encontrado;
- erro externo;
- preenchimento manual.

---

142. Endereço

Usuário deve poder corrigir endereço quando integração automática não resolver corretamente, conforme regra do sistema.

---

143. Notificações

Toasts ou alerts devem ser usados para feedback adequado.

Evitar toast para toda pequena interação.

Erros de formulário normalmente são melhores próximos ao campo.

---

144. Erros Técnicos

Não mostrar:

AxiosError
Network Error stack
Exception
SQL
stack trace

ao usuário final.

---

145. Network Error

Diferenciar erro de rede de erro de negócio quando possível.

Exemplo:

Não foi possível conectar ao servidor.

em vez de:

Produto inválido.

---

146. Offline

Se aplicação não possui suporte offline, não fingir sucesso em ações que dependem do servidor.

---

147. Retry

Retries automáticos devem ser avaliados.

GET pode permitir retry.

Operações críticas de escrita exigem cuidado, especialmente:

pedido;
pagamento;
estoque.

---

148. Error Boundary

Utilizar Error Boundary para erros inesperados de renderização quando apropriado.

Isso não substitui tratamento de erros assíncronos da API.

---

149. Suspense

Utilizar apenas se o projeto já estiver estruturado para isso ou houver justificativa.

Não misturar padrões de carregamento sem necessidade.

---

150. Testabilidade

Código Front-End deve ser escrito de forma testável.

Evitar dependências escondidas e funções gigantes.

Separar lógica pura quando isso facilitar testes.

---

151. Testes

Quando houver suíte de testes Front-End, manter preferência por testes de comportamento.

Testar:

renderização relevante;
interação;
loading;
erro;
sucesso;
estado;
acessibilidade;
integração entre componente e hook.

---

152. Não Testar Detalhe Interno

Evitar testar:

state interno;
nome de função privada;
quantidade de renders;
estrutura exata de divs.

quando isso não faz parte do comportamento.

---

153. Seletores de Teste

Preferir seletores que representam experiência do usuário:

role;
label;
texto;
name.

"data-testid" deve ser usado quando não houver alternativa semântica adequada.

---

154. Acessibilidade e Testes

Preferir localizar:

getByRole
getByLabelText
getByText

quando a ferramenta utilizada suportar esses conceitos.

Isso também melhora acessibilidade.

---

155. Mocks no Front-End

Mockar fronteiras externas.

Exemplo:

API
browser API específica
serviço externo

Não mockar o componente inteiro que deveria estar sob teste.

---

156. Dados dos Testes

Nunca usar dados pessoais reais.

Utilizar dados fictícios.

---

157. E2E

E2E deve testar fluxos críticos completos.

Exemplos:

Guest adiciona produto ao carrinho;
cliente faz login;
cliente finaliza pedido;
admin gerencia produto.

Não usar E2E para testar cada pequena variante visual.

---

158. Segurança em E2E

Nunca versionar:

senha real;
API Key;
token;
cookie;
dados financeiros.

---

159. Build

Antes de concluir alteração relevante, executar:

npm run build

ou comando equivalente real do projeto.

Corrigir erros TypeScript antes de considerar concluído.

---

160. Lint

Quando existir script de lint:

npm run lint

deve ser executado após mudanças relevantes.

Não desabilitar regra de lint apenas para evitar correção.

---

161. Testes Locais

Quando existirem:

npm test

ou:

npm run test

executar a suíte afetada.

Utilize sempre o script real do "package.json".

---

162. TypeScript Errors

É proibido resolver erros utilizando:

// @ts-ignore
// @ts-nocheck
as any

como solução padrão.

Corrigir a tipagem corretamente.

---

163. ESLint Disable

Não utilizar:

eslint-disable

sem justificativa específica.

Não desabilitar arquivo inteiro para resolver um warning.

---

164. Vite

Manter configuração Vite simples.

Não alterar:

porta;
proxy;
build target;
aliases;
plugins;

sem necessidade relacionada à tarefa.

---

165. Desenvolvimento Local

URLs locais devem vir da configuração apropriada.

Não alterar código de produção somente para fazê-lo funcionar na máquina do agente.

---

166. Docker

Se o Front-End roda em Docker:

- preservar multi-stage build se existente;
- não embutir secrets;
- manter build reproduzível;
- respeitar porta configurada.

---

167. Deploy

Alterações no Front-End devem considerar que o build é estático quando aplicável.

Variáveis "VITE_*" normalmente são incorporadas durante o build.

Não assumir que mudar variável no servidor depois do build atualizará automaticamente o bundle.

---

168. Cache de Deploy

Se nova versão não aparecer após deploy, investigar:

imagem Docker;
tag;
cache;
browser;
CDN;
service worker;
workflow;
container executado.

Não modificar código aleatoriamente para forçar atualização.

---

169. API Versioning

Se Backend utiliza versionamento:

/api/v1
/api/v2

services devem respeitar a versão correta.

Não atualizar endpoint sem verificar contrato.

---

170. Compatibilidade com Backend

Antes de modificar consumo de API:

1. verificar endpoint;
2. verificar método HTTP;
3. verificar request;
4. verificar response;
5. verificar headers;
6. verificar códigos de erro;
7. verificar autenticação;
8. verificar Tenant.

---

171. Status Codes

Não assumir que todo sucesso é "200".

Pode existir:

200 OK
201 Created
204 No Content

Tratar conforme contrato real.

---

172. 204 No Content

Não tentar fazer parse de JSON quando resposta é "204".

---

173. DTO de Erro

Quando Backend possui padrão de erro centralizado, criar um mapper único no Front-End.

Evitar interpretar erro diferentemente em cada tela.

---

174. Correlation ID

Se Backend retorna Correlation ID e ele é útil para suporte, pode ser preservado.

Não necessariamente deve ser exibido ao usuário em todas as situações.

---

175. Observabilidade

Quando existir ferramenta de observabilidade Front-End:

- não enviar secrets;
- evitar dados pessoais desnecessários;
- registrar contexto técnico útil;
- evitar logs excessivos.

---

176. Analytics

Eventos de analytics devem representar ações úteis.

Evitar enviar dados pessoais sensíveis ou tokens.

---

177. LGPD

O Front-End deve minimizar exposição de dados pessoais.

Não manter informações além do necessário.

Não registrar dados sensíveis em console ou analytics.

---

178. Dados Financeiros

Nunca armazenar dados completos de cartão quando integração utilizar gateway externo.

Seguir fluxo seguro do provedor.

---

179. Asaas

Integração com Asaas deve ocorrer prioritariamente no Backend.

O Front-End consome contratos internos do CloudShopping.

Não expor API Key do Asaas no navegador.

---

180. Uploads

Uploads devem validar na UI:

- extensão;
- tamanho;
- quantidade;

quando aplicável.

Backend continua responsável pela validação definitiva.

---

181. File Input

Não confiar no MIME informado pelo navegador como única validação de segurança.

---

182. Pré-visualização

Ao criar URL temporária com:

URL.createObjectURL(...)

liberar quando não for mais necessária:

URL.revokeObjectURL(...)

quando apropriado.

---

183. Modais

Modal deve possuir:

- fechamento previsível;
- suporte a teclado;
- foco;
- overlay;
- ações claras.

Não criar modal dentro de loops desnecessariamente.

---

184. Drawer

Drawers como CartDrawer devem funcionar em mobile.

Verificar:

- scroll;
- foco;
- overlay;
- fechamento;
- viewport pequena.

---

185. Z-Index

Não resolver conflitos adicionando valores cada vez maiores indiscriminadamente.

Manter escala consistente.

---

186. Scroll

Não bloquear scroll global sem restaurá-lo corretamente.

---

187. Navegação Mobile

Menu mobile deve:

- abrir;
- fechar;
- responder a mudança de rota;
- funcionar com teclado quando possível.

---

188. Loading Skeleton

Skeleton deve aproximar estrutura visual real.

Não é obrigatório em toda requisição.

---

189. Lazy Images

Imagens fora da primeira viewport podem utilizar:

loading="lazy"

quando apropriado.

---

190. SEO

Para páginas públicas, considerar:

title;
description;
headings;
semântica;
URLs legíveis.

quando SEO fizer parte do escopo.

---

191. SPA

Em SPA, recarregar rota diretamente não deve resultar em 404 do servidor.

Configuração de deploy deve fornecer fallback para "index.html" quando necessário.

---

192. Links Internos

Utilizar mecanismo de navegação do React Router, se existente.

Evitar:

<a href="/products">

para navegação interna quando isso força reload desnecessário.

---

193. React Router

Não criar router paralelo.

Rotas devem seguir estrutura já existente.

---

194. Componentes de Rota

Não carregar todas as regras no arquivo de routes.

Routes definem navegação, não negócio.

---

195. Guards

Guards no Front-End melhoram UX.

Autorização real deve continuar no Backend.

---

196. Cliente x Visitante

A UI deve distinguir corretamente:

Guest;
Lead;
B2C;
B2B;
Authenticated User.

quando esses conceitos fizerem parte do domínio.

Não assumir que todo Customer possui login.

---

197. B2B

Fluxos B2B podem possuir regras diferentes de B2C.

Não duplicar componente inteiro quando pequenas diferenças podem ser parametrizadas.

Mas também não criar componente gigante cheio de condições B2B/B2C.

---

198. CustomerType

Utilizar o contrato oficial.

Não inferir tipo de cliente apenas verificando se existe CNPJ.

---

199. Product

A UI deve considerar propriedades reais do produto:

ativo;
estoque;
preço;
imagem;
SKU;
categoria;
Tenant.

conforme DTO.

---

200. SKU

SKU é identificador comercial.

Não assumir automaticamente que SKU é ID técnico.

---

201. IDs

Não misturar:

ProductId
SKU
CustomerId
UserId
SessionToken
TenantId
BranchId

São conceitos diferentes.

---

202. Normalização de IDs

Não converter identificador para number/string arbitrariamente sem verificar contrato.

---

203. GUID

GUID deve ser tratado como identificador opaco.

Não implementar regra baseada no conteúdo textual do GUID.

---

204. URLs com IDs

Utilizar encode apropriado para parâmetros.

Não concatenar input livre sem encoding.

---

205. Query Strings

Construir query strings com ferramentas apropriadas.

Evitar concatenação frágil.

---

206. Paginação Server-Side

Manter parâmetros claros:

page
pageSize
sort
filter

conforme contrato real.

---

207. Ordenação

Não assumir ordenação do Backend se ela não estiver contratada.

---

208. Cache de Catálogo

Produtos podem ser cacheados.

Mas preço e estoque podem exigir atualização.

Não assumir cache indefinido.

---

209. staleTime

Configurar "staleTime" conforme natureza do dado.

Exemplo conceitual:

categorias -> mais estáveis
estoque -> mais dinâmico
pagamento -> altamente dinâmico

Não utilizar o mesmo valor cegamente para tudo.

---

210. refetchOnWindowFocus

Avaliar por tipo de dado.

Pode ser útil para dados dinâmicos.

Não desabilitar globalmente apenas porque uma tela faz requests extras.

---

211. Retry do React Query

GET pode possuir retry.

Mutations críticas devem ser analisadas individualmente.

Não retry automático de pagamento sem idempotência adequada.

---

212. Cancelamento de Query

Query que não é mais usada deve permitir cancelamento quando stack suportar.

---

213. React Strict Mode

Não remover Strict Mode apenas porque um efeito está executando duas vezes em desenvolvimento.

Corrigir efeitos incorretos.

---

214. Efeitos Idempotentes

Effects devem suportar comportamento do React em desenvolvimento.

Evitar efeitos que criam registros automaticamente sem proteção.

---

215. Chamada de API Durante Render

É proibido fazer requisição diretamente durante render.

Errado:

const result = productService.list();

quando isso dispara request a cada render.

Use hook/React Query.

---

216. Criação de Objeto Durante Render

Objetos simples podem ser criados.

Mas evite instanciar clients pesados ou serviços globais repetidamente.

---

217. Context Provider

Providers devem ser colocados em nível adequado.

Não envolver cada componente individualmente com o mesmo Provider.

---

218. Provider Hell

Se houver muitos Providers encadeados, considerar composição organizada.

Não adicionar Context para toda nova feature automaticamente.

---

219. Custom Hooks

Quando lógica de componente cresce em:

requests;
mutations;
estado;
efeitos;

considere extrair hook específico.

---

220. Utils

"utils" deve conter funções genéricas reais.

Não jogar lógica de domínio aleatória em "utils".

---

221. Helpers

Helpers devem ser determinísticos quando possível.

Funções puras são preferíveis para:

formatação;
mapping;
normalização.

---

222. Mappers

Quando response do Backend precisa ser transformado para modelo de UI, mapper explícito pode ser utilizado.

Não esconder transformação grande dentro do JSX.

---

223. JSX

JSX deve permanecer legível.

Evitar:

{condition ? x ? a : b : y ? c : d}

Extrair lógica quando necessário.

---

224. Condicionais

Condições importantes devem possuir nomes semânticos.

Preferir:

const canCheckout = hasItems && !isLoading && isCartValid;

---

225. Early Return

Utilizar early return para estados simples:

if (isLoading) return <Skeleton />;
if (error) return <ErrorState />;

quando melhora legibilidade.

---

226. Fragmentos

Não adicionar "div" sem necessidade apenas para envolver JSX.

Use Fragment quando apropriado.

---

227. CSS Global

Evitar colocar regra específica de componente em CSS global.

---

228. Tailwind e Estado

Utilizar classes condicionais de maneira legível.

Se lógica ficar complexa, considerar helper já existente.

---

229. Biblioteca de Classes

Se projeto utiliza "clsx", "classnames" ou equivalente, reutilizar.

Não adicionar outra ferramenta com mesma finalidade.

---

230. Component Variants

Quando já existir utilitário de variants, seguir o padrão.

---

231. Design System

Componentes compartilhados devem seguir design system existente.

Não inventar novo padrão local para cada feature.

---

232. Ações Críticas

Botões críticos devem comunicar claramente ação.

Preferir:

Cancelar pedido
Excluir endereço
Finalizar compra

em vez de apenas:

Confirmar
OK

quando contexto não for óbvio.

---

233. Confirmação de Pagamento

Não mostrar “Pagamento realizado” antes da confirmação real.

---

234. Navegação Pós-Ação

Após mutation, navegar somente após sucesso confirmado quando esse for o comportamento esperado.

---

235. Race em Navegação

Não atualizar estado de componente desmontado manualmente quando biblioteca já gerencia ciclo de vida.

---

236. AbortController

Quando services suportarem, propagar "AbortSignal".

---

237. Axios Interceptors

Interceptors devem tratar preocupações globais.

Evitar colocar lógica específica de feature neles.

Bom:

JWT
Tenant
Correlation
erro global técnico

Ruim:

se endpoint for cart então abrir drawer

---

238. Refresh Token

Se aplicação tiver refresh token:

- centralizar fluxo;
- evitar múltiplos refresh simultâneos;
- evitar loop infinito;
- limpar sessão em falha definitiva.

Não implementar refresh sem contrato real do Backend.

---

239. CORS

CORS é configuração do servidor.

Não tentar “resolver CORS” adicionando headers aleatórios no React.

---

240. Segurança de Headers

Headers como CSP, HSTS e CORS pertencem majoritariamente à infraestrutura/servidor.

Front-End deve ser compatível, não fingir implementá-los no JavaScript.

---

241. Secrets

Regra absoluta:

NUNCA colocar segredo real no Front-End.

Se browser precisa acessar algo, considere que usuário também consegue acessar.

---

242. Configuração por Ambiente

Ambientes podem possuir:

development
test
staging
production

Não misturar endpoints entre ambientes.

---

243. Produção

Nunca incluir:

localhost
token de teste
usuário administrador fixo
mock ativado

no build de produção sem intenção explícita.

---

244. Mocks de Desenvolvimento

Mocks devem estar claramente isolados.

Não adicionar fallback silencioso que retorna dados falsos quando API real falha.

---

245. Feature Flags

Se existirem, centralizar leitura.

Não espalhar checagem de variável de ambiente por dezenas de componentes.

---

246. Compatibilidade de Navegador

Não utilizar API experimental sem verificar suporte ou necessidade de polyfill.

---

247. Intl

Preferir "Intl" para:

moeda;
datas;
números.

em vez de implementações manuais.

---

248. Performance de Renderização

Listas muito grandes podem exigir:

paginação;
virtualização;
memoização.

Mas medir necessidade primeiro.

---

249. Accessibility Loading

Loading deve informar estado adequadamente quando necessário.

Exemplo:

aria-busy
aria-live

em componentes críticos.

---

250. Mensagens Dinâmicas

Notificações importantes podem utilizar região "aria-live" quando apropriado.

---

251. Contraste

Não criar texto ilegível para seguir estética.

Acessibilidade tem prioridade.

---

252. Hover

Não depender apenas de hover para transmitir informação essencial.

Mobile não possui hover equivalente.

---

253. Touch Targets

Botões e controles mobile devem possuir área de toque adequada.

---

254. Tabelas

Dados tabulares devem utilizar "<table>" quando realmente forem tabelares.

Em mobile, considerar experiência responsiva.

---

255. Paginação de Tabela

Paginação deve preservar filtros e ordenação quando apropriado.

---

256. Form Submission

Utilizar "<form onSubmit>" para formulários reais quando apropriado.

Isso melhora comportamento de teclado e acessibilidade.

---

257. Enter

Formulário deve considerar submissão via Enter.

Não depender somente de click.

---

258. Campos Disabled

Diferenciar:

disabled
readonly

conforme semântica real.

---

259. Reset de Formulário

Após sucesso, limpar apenas quando UX exigir.

Não apagar dados imediatamente após falha.

---

260. Preservação de Dados

Em falha de API, preservar valores preenchidos pelo usuário.

---

261. Modal com Formulário

Não fechar automaticamente modal se operação falhou.

---

262. Erro de Campo Backend

Quando Backend retornar erro associado a campo, mapear para formulário quando possível.

---

263. Erro Global Backend

Erros não associados a campo devem possuir área global apropriada.

---

264. Estado Inicial

Componentes devem possuir estado inicial previsível.

Evitar:

undefined durante alguns renders

quando tipo pode representar melhor o estado.

---

265. Union Types

Para estados complexos, considerar union discriminada.

Exemplo:

type ViewState =
  | { status: "loading" }
  | { status: "error"; error: ApiError }
  | { status: "success"; data: Product[] };

quando isso realmente simplificar o código.

---

266. Boolean Explosion

Evitar vários booleans inconsistentes:

isLoading
isLoaded
hasError
isSuccess

quando biblioteca já oferece estado coerente.

---

267. Feature Completa

Ao implementar nova feature, verificar todas as camadas Front-End necessárias:

Types
   ↓
Service
   ↓
Hook
   ↓
Page/Component
   ↓
Loading
   ↓
Error
   ↓
Empty
   ↓
Success

---

268. Fluxo de API

Antes de concluir nova integração:

Request correto?
Response correto?
Tenant correto?
Auth correto?
Erro tratado?
Loading tratado?
Cache tratado?
Cancelamento tratado?

---

269. Não Alterar Backend sem Necessidade

Ao encontrar dificuldade no Front-End, não mudar contrato do Backend automaticamente.

Primeiro verificar se consumo está incorreto.

---

270. Backend é Fonte de Verdade

Para:

preço;
estoque;
permissão;
status;
pagamento;
regra de negócio;
Tenant;
pedido.

Backend é autoridade definitiva.

---

271. Segurança por Ocultação

Esconder botão NÃO é autorização.

Exemplo:

Usuário não vê "Excluir"

melhora UX.

Mas Backend ainda deve rejeitar request não autorizado.

---

272. Mudanças Mínimas

Faça a menor alteração necessária para resolver a tarefa.

Evitar:

- refatoração global;
- mudança de stack;
- nova biblioteca sem necessidade;
- reorganização de pastas;
- novo padrão concorrente;
- alteração de design não solicitada.

---

273. Respeitar Código Existente

Antes de criar um novo padrão, examine pelo menos uma implementação semelhante.

Preservar consistência é normalmente melhor que introduzir preferência pessoal do agente.

---

274. Refatoração

Refatorar quando:

- reduz complexidade real;
- remove duplicação relevante;
- melhora separação;
- corrige problema arquitetural;
- facilita manutenção.

Não refatorar apenas por estética.

---

275. Compatibilidade

Não quebrar comportamento existente que não faz parte do escopo.

---

276. Regressão

Correção de bug deve considerar cenário que causou o problema.

Quando existir suíte de teste, adicionar teste de regressão apropriado.

---

277. Não Silenciar Erros

É proibido:

try {
  ...
} catch {
}

sem tratamento intencional.

Erros não devem desaparecer silenciosamente.

---

278. Catch

Capturar erro somente quando existe algo útil a fazer:

transformar;
mostrar;
registrar;
recuperar;
limpar estado.

---

279. Promise

Não ignorar Promise.

Evitar:

service.save(data);
navigate("/success");

quando navegação depende do sucesso.

---

280. Estado Após Unmount

Evitar soluções manuais frágeis como:

let isMounted = true;

se stack utilizada já gerencia isso corretamente.

---

281. Código Duplicado de API

Se vários services repetem:

headers;
base URL;
auth;
Tenant;
error handling;

a responsabilidade deve estar no "apiClient".

---

282. HTTP Client Único

Não possuir vários Axios instances sem necessidade.

Exceção possível: integrações realmente diferentes com políticas próprias.

---

283. APIs Externas Diretas

Evitar navegador chamar fornecedor externo quando Backend pode atuar como gateway seguro.

Especialmente quando envolve autenticação privada.

---

284. Cálculo de Frete

Front-End solicita cálculo.

Backend define resultado.

Não reimplementar tabela completa de frete em JavaScript sem necessidade arquitetural.

---

285. Cupons

Front-End pode mostrar desconto.

Backend valida cupom, regras e valor final.

---

286. Promoções

Não confiar em promoção calculada somente no cliente.

---

287. Estoque Visual

Estado exibido pode ficar desatualizado.

Tratar resposta final do Backend no checkout.

---

288. Concorrência

Se outro usuário comprar último item antes, Front-End deve tratar o conflito.

Não assumir que estoque exibido garante reserva.

---

289. Session Expiration

Sessões Guest e Auth podem expirar.

Tratar expiração sem quebrar aplicação inteira.

---

290. Recuperação

Quando seguro, permitir usuário tentar novamente após falha temporária.

---

291. Não Repetir Pagamento

Botão de pagamento deve ser protegido contra clicks repetidos.

---

292. Feedback de Processamento

Em ação crítica, usuário deve saber que operação está em processamento.

---

293. Estado Ambíguo

Se request de pagamento perder conexão após envio, não assumir automaticamente sucesso ou falha.

Consultar Backend quando arquitetura permitir.

---

294. Carrinho Persistente

Persistência de carrinho deve seguir regra do Backend.

Não inventar validade no Front-End se servidor define expiração.

---

295. Carrinho Guest

Guest deve continuar identificado pelo mecanismo definido.

Não criar novo carrinho a cada reload.

---

296. Merge de Carrinho

Se login suporta merge de Guest Cart com Customer Cart, lógica definitiva deve estar no Backend.

Front-End apenas inicia fluxo e atualiza estado.

---

297. Cache Pós-Login

Após login:

- invalidar dados Guest quando necessário;
- carregar contexto do cliente;
- atualizar carrinho;
- atualizar queries privadas.

---

298. Cache Pós-Logout

Após logout:

- remover queries privadas;
- impedir flash de dados do usuário anterior.

---

299. Admin e Tenant

Administrador deve operar no Tenant correto.

Não manter Tenant antigo ao alternar contexto administrativo.

---

300. Definition of Done

Uma tarefa Front-End só pode ser considerada concluída quando:

- código compila;
- TypeScript está correto;
- lint relevante passa;
- build passa;
- testes relevantes passam quando existirem;
- API utilizada existe;
- request está correto;
- response está corretamente tipado;
- loading foi tratado;
- erro foi tratado;
- empty state foi considerado;
- mobile foi considerado;
- Tenant foi considerado;
- autenticação foi considerada;
- Guest foi considerado quando aplicável;
- cache foi atualizado;
- não existem secrets;
- não existem logs de debug;
- não foi introduzida duplicação desnecessária;
- arquitetura existente foi respeitada.

---

301. Checklist de Componente

[ ] Possui responsabilidade clara?
[ ] Props estão tipadas?
[ ] Possui lógica demais?
[ ] Pode reutilizar componente existente?
[ ] Está acessível?
[ ] Funciona em mobile?
[ ] Loading está tratado?
[ ] Error está tratado?
[ ] Não chama Axios diretamente sem necessidade?
[ ] Não contém regra de negócio definitiva?

---

302. Checklist de Hook

[ ] Possui responsabilidade clara?
[ ] Reutiliza React Query quando dado vem do servidor?
[ ] Não duplica estado?
[ ] Não utiliza useEffect desnecessariamente?
[ ] Service está separado?
[ ] Cache está correto?
[ ] Tenant faz parte da chave quando necessário?
[ ] Erro está normalizado?
[ ] Cancellation está considerada?

---

303. Checklist de Service

[ ] Endpoint existe?
[ ] Método HTTP está correto?
[ ] Request está tipado?
[ ] Response está tipado?
[ ] Usa apiClient central?
[ ] Não duplica baseURL?
[ ] Não duplica JWT?
[ ] Não duplica X-Tenant-ID?
[ ] Não contém regra visual?

---

304. Checklist de React Query

[ ] Query Key é estável?
[ ] Tenant está isolado?
[ ] Query é realmente leitura?
[ ] Mutation é realmente escrita?
[ ] Cache foi invalidado corretamente?
[ ] Não invalida tudo?
[ ] Retry é seguro?
[ ] staleTime é coerente?
[ ] Optimistic Update possui rollback?

---

305. Checklist de Autenticação

[ ] Token possui fonte única?
[ ] Logout limpa dados privados?
[ ] 401 está tratado?
[ ] 403 está tratado separadamente?
[ ] Não há secret no Front-End?
[ ] Cache privado é removido?
[ ] Redirecionamento não cria loop?

---

306. Checklist Multi-Tenant

[ ] X-Tenant-ID é enviado corretamente?
[ ] Tenant não está hardcoded?
[ ] Cache possui isolamento?
[ ] Mudança de Tenant limpa contexto anterior?
[ ] Branch está separado de Tenant?
[ ] Carrinho pertence ao Tenant correto?
[ ] Dados do Tenant anterior não aparecem?

---

307. Checklist Guest

[ ] Sessão Guest é criada corretamente?
[ ] SessionToken possui fonte única?
[ ] Token não é recriado em render?
[ ] Reload preserva sessão quando necessário?
[ ] Carrinho Guest funciona?
[ ] Login atualiza o contexto corretamente?
[ ] Não há acesso à sessão de outro visitante?

---

308. Checklist Carrinho

[ ] AddItem atualiza UI?
[ ] UpdateQuantity atualiza UI?
[ ] RemoveItem atualiza UI?
[ ] Loading existe?
[ ] Double click está protegido?
[ ] Quantidade inválida está bloqueada?
[ ] Estoque insuficiente é tratado?
[ ] Cache está sincronizado?
[ ] Guest está tratado?
[ ] Tenant está tratado?

---

309. Checklist Formulário

[ ] Labels existem?
[ ] Campos estão tipados?
[ ] Validação de UX existe?
[ ] Backend continua sendo autoridade?
[ ] Erros de campo são exibidos?
[ ] Erro global é exibido?
[ ] Submit duplicado está bloqueado?
[ ] Valores permanecem após falha?
[ ] Mobile está adequado?

---

310. Checklist de Segurança

[ ] Nenhum secret está no código?
[ ] Nenhuma API Key privada está em VITE_*?
[ ] Nenhum dado sensível está no console?
[ ] dangerous HTML foi evitado?
[ ] Autorização não depende apenas da UI?
[ ] Dados privados são limpos no logout?
[ ] Links externos estão seguros?
[ ] Dados pessoais foram minimizados?

---

311. Checklist de UI/UX

[ ] Loading?
[ ] Success?
[ ] Error?
[ ] Empty?
[ ] Disabled?
[ ] Mobile?
[ ] Keyboard?
[ ] Focus?
[ ] Feedback da ação?
[ ] Consistência visual?

---

312. Fluxo Obrigatório do Agente

Sempre seguir:

1. Ler este AGENTS.md
        |
        v
2. Identificar a feature
        |
        v
3. Ler implementação existente
        |
        v
4. Localizar componente semelhante
        |
        v
5. Localizar hook existente
        |
        v
6. Localizar service existente
        |
        v
7. Confirmar contrato da API
        |
        v
8. Confirmar Auth / Tenant / Guest
        |
        v
9. Planejar menor alteração
        |
        v
10. Implementar
        |
        v
11. Validar loading/error/empty/success
        |
        v
12. Validar mobile/acessibilidade
        |
        v
13. Executar lint
        |
        v
14. Executar testes
        |
        v
15. Executar build
        |
        v
16. Relatar o resultado real

---

313. Regra para Agentes de IA

Ao receber uma tarefa, o agente NÃO deve começar imediatamente criando arquivos.

Primeiro deve entender:

O que já existe?
Qual padrão o projeto utiliza?
Qual contrato Backend existe?
Qual estado já é gerenciado?
Qual componente já resolve algo parecido?
Qual é a menor alteração?

---

314. Proibição de Implementação por Suposição

É proibido criar implementação baseada apenas em descrição textual quando código real está disponível.

Exemplo:

Se a tarefa disser:

adicione um botão para remover produto do carrinho

antes de implementar, localizar:

CartDrawer
CartItem
useCart
cartService
endpoint DELETE/PUT correspondente
tipos do carrinho

---

315. Não Criar Endpoint

Front-End não cria endpoint.

Se endpoint necessário não existir, registrar a dependência.

Não inventar:

DELETE /api/cart/items/{id}

apenas porque parece apropriado.

---

316. Não Inventar Response

Não assumir:

{
  success: true,
  data: ...
}

se a API não utiliza esse contrato.

---

317. Não Inventar Campos

Não adicionar em TypeScript:

product.stockQuantity

sem verificar se API realmente fornece esse campo.

---

318. Não Silenciar Incompatibilidade

Se Backend e Front-End possuem contratos incompatíveis, corrigir conscientemente.

Não esconder com:

as any

---

319. Arquitetura Preferencial

Quando compatível com a base existente:

Pages
   ↓
UI Components
   ↓
Feature Hooks
   ↓
React Query
   ↓
Services
   ↓
Axios Client
   ↓
CloudShopping API

Preocupações globais:

Auth
Tenant
Guest
Error Handling
QueryClient
Routing

devem permanecer centralizadas.

---

320. Regra Final

O agente deve otimizar para:

correção
+
consistência
+
experiência do usuário
+
segurança
+
tipagem
+
arquitetura
+
acessibilidade
+
responsividade
+
manutenibilidade

e NÃO para:

quantidade de código
+
criação de componentes
+
abstração excessiva
+
bibliotecas novas
+
refatoração desnecessária

Sempre prefira a solução mais simples que respeite:

arquitetura existente
+
contrato da API
+
regras do negócio
+
experiência do usuário

«O Front-End deve representar corretamente o estado do sistema, facilitar a interação do usuário e nunca assumir responsabilidades que pertencem ao Backend.»