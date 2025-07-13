import sys
import os

TEMPLATE_FILE = 'README.template.md'
OUTPUT_FILE = 'README.md'
TREE_FILE = 'project_tree.txt'
PLACEHOLDER = '{{project_tree}}'

def main():
    if not os.path.exists(TEMPLATE_FILE):
        print(f"--- ERRO: Arquivo de template '{TEMPLATE_FILE}' não encontrado! Verifique se ele foi commitado. ---")
        sys.exit(1)

    if not os.path.exists(TREE_FILE):
        print(f"--- ERRO: Arquivo de árvore '{TREE_FILE}' não foi gerado! ---")
        sys.exit(1)

    with open(TEMPLATE_FILE, 'r', encoding='utf-8') as f:
        template_content = f.read()

    with open(TREE_FILE, 'r', encoding='utf-8') as f:
        tree_content = f.read()

    if PLACEHOLDER not in template_content:
        print(f"--- ERRO: Placeholder '{PLACEHOLDER}' não encontrado em '{TEMPLATE_FILE}'! ---")
        sys.exit(1)
        
    tree_block = f'```\n{tree_content}```'
    
    final_readme = template_content.replace(PLACEHOLDER, tree_block)

    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write(final_readme)

    print(f"--- Sucesso: '{OUTPUT_FILE}' foi gerado com sucesso. ---")

if __name__ == "__main__":
    main()
