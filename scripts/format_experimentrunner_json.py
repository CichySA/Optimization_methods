import json
import glob
import re
import sys


def format_file(path):
    try:
        with open(path, 'r', encoding='utf-8-sig') as f:
            data = json.load(f)
    except Exception as e:
        print(f"ERROR parsing {path}: {e}")
        return False

    dumped = json.dumps(data, indent=2, ensure_ascii=False)

    if (
        isinstance(data, dict)
        and data.get('Parameters')
        and isinstance(data['Parameters'], dict)
        and data['Parameters'].get('ParameterGrid')
        and isinstance(data['Parameters']['ParameterGrid'], dict)
    ):
        pg = data['Parameters']['ParameterGrid']
        m = re.search(r'"ParameterGrid"\s*:\s*{', dumped)
        if m:
            brace_open = dumped.find('{', m.end()-1)
            i = brace_open
            depth = 1
            while i + 1 < len(dumped) and depth > 0:
                i += 1
                c = dumped[i]
                if c == '{':
                    depth += 1
                elif c == '}':
                    depth -= 1
            end = i
            block = dumped[brace_open:end+1]
            new_block = block
            for key, val in pg.items():
                compact = json.dumps(val, ensure_ascii=False)
                pattern = r'"' + re.escape(key) + r'"\s*:\s*\[[\s\S]*?\]'
                new_block = re.sub(pattern, '"' + key + '": ' + compact, new_block)
            dumped = dumped[:brace_open] + new_block + dumped[end+1:]

    if not dumped.endswith('\n'):
        dumped += '\n'

    try:
        with open(path, 'r', encoding='utf-8-sig') as f:
            old = f.read()
    except Exception:
        old = None

    if old == dumped:
        print(f"Unchanged: {path}")
        return False

    try:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(dumped)
        print(f"Formatted: {path}")
        return True
    except Exception as e:
        print(f"ERROR writing {path}: {e}")
        return False


def main():
    files = glob.glob('Experiments/**/experimentrunner.json', recursive=True)
    if not files:
        print("No experimentrunner.json files found.")
        return 0
    modified = 0
    for path in sorted(files):
        try:
            if format_file(path):
                modified += 1
        except Exception as e:
            print(f"Error formatting {path}: {e}")
    print(f"Done. Modified {modified}/{len(files)} files.")
    return 0


if __name__ == '__main__':
    sys.exit(main())
