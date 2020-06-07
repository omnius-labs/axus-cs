init-tools:
	sh ./eng/init-tools.sh

gen-code:
	sh ./eng/gen-code.sh

test:
	sh ./eng/test.sh

update:
	sh ./eng/update-submodule.sh
	sh ./eng/update-tools.sh

format:
	dotnet tool restore
	dotnet tool run dotnet-format

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./pub

.PHONY: all test clean
