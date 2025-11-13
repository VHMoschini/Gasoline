# Setup do Joystick Virtual para Mobile

## Arquivos Criados

1. **VirtualJoystick.cs** - Controla o joystick virtual com touch input
2. **InputManager.cs** - Gerenciador que unifica input de PC e Mobile
3. **UIMobileGameplay.cs** - Gerencia a UI mobile (atualizado)
4. **CarPhysics.cs** - Modificado para usar o InputManager

## Instruções de Setup na Unity

### Passo 1: Criar a UI do Joystick

1. No Canvas da sua cena de gameplay, clique com botão direito → **UI → Panel**
2. Renomeie para "JoystickArea"
3. Configure o RectTransform:
   - Anchor: Bottom-Left
   - Position: (0, 0)
   - Width: 400
   - Height: 400
   - Deixe transparente (remova o componente Image ou deixe Alpha = 0)

### Passo 2: Criar o Background do Joystick

1. Clique direito em "JoystickArea" → **UI → Image**
2. Renomeie para "JoystickBackground"
3. Configure:
   - Width: 150
   - Height: 150
   - Color: Branco semi-transparente (Alpha: 80)
   - Opcional: Use um sprite circular para melhor aparência
   - **Desative inicialmente** (será ativado quando o jogador tocar)

### Passo 3: Criar o Handle do Joystick

1. Clique direito em "JoystickBackground" → **UI → Image**
2. Renomeie para "JoystickHandle"
3. Configure:
   - Width: 80
   - Height: 80
   - Color: Branco ou outra cor destacada (Alpha: 200)
   - Anchor: Center
   - Position: (0, 0)

### Passo 4: Configurar os Scripts

#### A) No objeto JoystickArea:
1. Adicione o componente **VirtualJoystick.cs**
2. Configure:
   - Joystick Background: Arraste o objeto "JoystickBackground"
   - Joystick Handle: Arraste o objeto "JoystickHandle"
   - Handle Range: 50 (ajuste conforme preferir)
   - Dead Zone: 0.1

#### B) Criar um GameObject vazio para o InputManager:
1. Crie um GameObject vazio na hierarquia (botão direito → Create Empty)
2. Renomeie para "InputManager"
3. Adicione o componente **InputManager.cs**
4. Configure:
   - Virtual Joystick: Arraste o objeto "JoystickArea" (que tem o VirtualJoystick.cs)
   - Is Mobile: Será detectado automaticamente, mas você pode marcar para testar no Editor

#### C) No Canvas ou em um GameObject UI:
1. Se já existe um objeto com UIMobileGameplay, use ele
2. Senão, crie um GameObject vazio e adicione **UIMobileGameplay.cs**
3. Configure:
   - Virtual Joystick: Arraste o objeto "JoystickArea"
   - Input Manager: Arraste o objeto "InputManager"

### Passo 5: Testar no Editor

Para testar o joystick no editor Unity:
1. No **InputManager**, marque "Is Mobile" como **true**
2. Entre no Play Mode
3. Clique e arraste na área do joystick para testar

### Passo 6: Build para Mobile

1. Vá em **File → Build Settings**
2. Selecione **Android** ou **iOS**
3. Click em **Switch Platform**
4. Configure o Player Settings:
   - Company Name
   - Product Name
   - Bundle Identifier (ex: com.suaempresa.gasoline)
5. Build e teste no dispositivo

## Customização Visual (Opcional)

### Adicionar Sprites Personalizados:
1. Importe suas imagens de joystick para a pasta Assets/Sprites/UI
2. Configure como Sprite (2D and UI)
3. No Inspector do JoystickBackground e JoystickHandle:
   - Componente Image → Source Image: Arraste seu sprite

### Ajustar Posição e Tamanho:
- Modifique Width/Height do JoystickArea para ajustar área tocável
- Modifique Handle Range no VirtualJoystick para ajustar sensibilidade
- Modifique Dead Zone para ajustar zona morta central

## Funcionalidades

✅ **Detecção automática de plataforma** - O joystick só aparece em mobile
✅ **Joystick flutuante** - Aparece onde você toca
✅ **Compatibilidade com PC** - Mantém WASD funcionando no PC
✅ **Dead zone configurável** - Evita movimentos não intencionais
✅ **Visual feedback** - O handle se move mostrando a direção

## Próximos Passos Sugeridos

1. **Adicionar botão de freio/drift** - Criar um botão UI para substituir a barra de espaço
2. **Adicionar botão de câmera** - Para alternar visões
3. **Otimizar para tablets** - Ajustar tamanhos para diferentes resoluções
4. **Adicionar feedback visual** - Efeitos quando o jogador toca
5. **Salvar configurações** - Permitir que o jogador ajuste sensibilidade

## Troubleshooting

**O joystick não aparece no mobile:**
- Verifique se o Canvas está configurado como "Screen Space - Overlay"
- Certifique-se que "Is Mobile" está detectando corretamente no InputManager

**O carro não se move:**
- Verifique se o InputManager está na cena
- Certifique-se que o VirtualJoystick está conectado ao InputManager
- Verifique se canMove = true no CarPhysics

**O joystick não responde ao toque:**
- Certifique-se que JoystickArea tem o componente VirtualJoystick
- Verifique se há um Event System na cena (Canvas cria automaticamente)
- Certifique-se que nenhum outro UI está bloqueando os toques
