import { createMDX } from 'fumadocs-mdx/next';
import { achNextConfig } from 'ach-fumadocs-theme/next';

const withMDX = createMDX();

// basePath = '/skill-creator' → docs.somiri.dev/skill-creator 경로에 매핑된다.
export default withMDX(achNextConfig({ repo: 'skill-creator' }));
