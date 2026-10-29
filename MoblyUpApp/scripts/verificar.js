// Checagens rápidas do código do app (rode com: npm run verificar)
// 1. Todo componente usado em JSX (<Nome ...>) foi importado ou declarado no arquivo
// 2. Todo styles.X usado numa tela existe no StyleSheet correspondente
// Pegam erros que só apareceriam com o app rodando (tela quebrando ou botão sem estilo).
const fs = require('fs');
const path = require('path');

const raiz = path.join(__dirname, '..', 'src');
let problemas = 0;

const arquivosJs = [];
(function varrer(dir) {
  for (const item of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, item.name);
    if (item.isDirectory()) varrer(p);
    else if (p.endsWith('.js')) arquivosJs.push(p);
  }
})(raiz);

// 1. Componentes importados
for (const arquivo of arquivosJs) {
  const codigo = fs.readFileSync(arquivo, 'utf8');
  const usados = new Set([...codigo.matchAll(/<([A-Z][A-Za-z0-9]*)[\s/>.]/g)].map((m) => m[1]));
  const declarados = new Set();
  for (const m of codigo.matchAll(/import\s+([\s\S]*?)\s+from/g)) {
    for (const nome of m[1].replace(/[{}*]/g, ' ').split(/[\s,]+/)) if (nome && nome !== 'as') declarados.add(nome);
  }
  for (const m of codigo.matchAll(/(?:const|function|let|class)\s+([A-Z][A-Za-z0-9]*)/g)) declarados.add(m[1]);

  for (const nome of usados) {
    if (!declarados.has(nome)) {
      console.log(`✗ ${path.relative(raiz, arquivo)}: <${nome}> não importado`);
      problemas++;
    }
  }
}

// 2. Estilos existentes
const telas = path.join(raiz, 'screens');
for (const pasta of fs.readdirSync(telas)) {
  const dir = path.join(telas, pasta);
  for (const nome of fs.readdirSync(dir).filter((n) => n.endsWith('Screen.js'))) {
    const tela = fs.readFileSync(path.join(dir, nome), 'utf8');
    const importStyles = tela.match(/import\s+\{\s*styles\s*\}\s+from\s+'\.\/(\w+)'/);
    const fonte = importStyles ? fs.readFileSync(path.join(dir, importStyles[1] + '.js'), 'utf8') : tela;
    const bloco = fonte.slice(fonte.indexOf('StyleSheet.create'));
    const definidos = new Set([...bloco.matchAll(/^\s{2}(\w+):\s*\{/gm)].map((m) => m[1]));
    const usados = new Set([...tela.matchAll(/styles\.(\w+)/g)].map((m) => m[1]));

    for (const u of usados) {
      if (!definidos.has(u)) {
        console.log(`✗ ${pasta}/${nome}: styles.${u} não existe`);
        problemas++;
      }
    }
  }
}

if (problemas) {
  console.log(`\n${problemas} problema(s) encontrado(s).`);
  process.exit(1);
}
console.log(`✓ ${arquivosJs.length} arquivos verificados: componentes importados e estilos existentes.`);
