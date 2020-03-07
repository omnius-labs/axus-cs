init-tools:
	sh ./eng/init-tools.sh

gen-code:
	sh ./eng/gen-code.sh

test:
	sh ./eng/run-test.sh

update-submodule:
	sh ./eng/update-submodule.sh

clean:
	rm -rf ./bin
	rm -rf ./tmp
	rm -rf ./publish

.PHONY: all test clean
