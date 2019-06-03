mkdir archive
git diff --diff-filter=AMCR --name-only HEAD | xargs -I % cp --parents % archive