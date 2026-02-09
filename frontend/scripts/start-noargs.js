#!/usr/bin/env node
import { spawn } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';
import { createRequire } from 'module';

const require = createRequire(import.meta.url);
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

try {
  const viteBin = require.resolve('vite/bin/vite.js', { paths: [path.resolve(__dirname, '..')] });
  const child = spawn(process.execPath, [viteBin], { stdio: 'inherit' });
  child.on('exit', code => process.exit(code));
} catch (err) {
  console.error('Could not resolve vite binary:', err.message);
  process.exit(1);
}
